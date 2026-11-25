using DemoExam2.Data;
using DemoExam2.Helpers;
using DemoExam2.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DemoExam2
{
    public partial class CatalogWindow : Window
    {
        private readonly User? _user;
        private List<Product> _products = new();

        private bool IsAdmin => _user?.IdRoleNavigation.Name == "Администратор";
        private bool CanFilter => IsAdmin || _user?.IdRoleNavigation.Name == "Менеджер";

        public CatalogWindow(User? user = null)
        {
            _user = user;
            InitializeComponent();
            UserName.Text = _user == null
                ? "Гость"
                : $"{_user.LastName} {_user.FirstName} {_user.MiddleName}";
            FilterPanel.Visibility = CanFilter ? Visibility.Visible : Visibility.Collapsed;
            AdminPanel.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                using var dbContext = new AppDbContext();
                _products = dbContext.Products
                    .Include(p => p.IdCategoryNavigation)
                    .Include(p => p.IdManufacturerNavigation)
                    .Include(p => p.IdFactoryNavigation)
                    .Include(p => p.IdUnitNavigation)
                    .ToList();

                if (FactoryBox.Items.Count == 0 && CanFilter)
                {
                    FactoryBox.Items.Add("Все поставщики");
                    foreach (var factory in dbContext.Factories.OrderBy(f => f.Name))
                    {
                        FactoryBox.Items.Add(factory.Name);
                    }
                    FactoryBox.SelectedIndex = 0;
                    SortBox.SelectedIndex = 0;
                }
            }
            catch
            {
                MessageHelper.ShowError("Не удалось загрузить товары. Проверьте подключение к базе данных.");
                return;
            }

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IEnumerable<Product> products = _products;

            if (CanFilter)
            {
                var search = SearchBox.Text.Trim();
                if (search != string.Empty)
                {
                    products = products.Where(p => string.Join(" ", p.Articul, p.Name, p.Description,
                        p.IdCategoryNavigation.Name, p.IdManufacturerNavigation.Name,
                        p.IdFactoryNavigation.Name, p.IdUnitNavigation.Name, p.Image)
                        .Contains(search, StringComparison.CurrentCultureIgnoreCase));
                }

                if (FactoryBox.SelectedIndex > 0)
                {
                    var factoryName = FactoryBox.SelectedItem.ToString();
                    products = products.Where(p => p.IdFactoryNavigation.Name == factoryName);
                }

                if (SortBox.SelectedIndex == 1)
                {
                    products = products.OrderBy(p => p.Count);
                }
                else if (SortBox.SelectedIndex == 2)
                {
                    products = products.OrderByDescending(p => p.Count);
                }
            }

            ProductList.ItemsSource = products.Select(p => new
            {
                Id = p.Id,
                Image = GetImagePath(p.Image),
                Title = $"{p.IdCategoryNavigation.Name} | {p.Name}",
                Description = "Описание товара: " + p.Description,
                Manufacturer = "Производитель: " + p.IdManufacturerNavigation.Name,
                Factory = "Поставщик: " + p.IdFactoryNavigation.Name,
                Price = p.Price,
                DiscountedPrice = p.Discount > 0
                    ? $"  {p.Price * (100m - p.Discount) / 100m:N2} руб."
                    : string.Empty,
                RowColor = p.Count <= 0
                    ? Brushes.LightBlue
                    : p.Discount > 15
                        ? new SolidColorBrush(Color.FromRgb(46, 139, 87))
                        : new SolidColorBrush(Color.FromRgb(127, 255, 0)),
                Unit = "Единица измерения: " + p.IdUnitNavigation.Name,
                CountText = "Количество на складе: " + p.Count,
                DiscountText = p.Discount == 0 ? string.Empty : $"Действующая скидка: {p.Discount}%"
            }).ToList();
        }

        private string GetImagePath(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "/Image/picture.png";
            }

            var relativePath = image.Replace('/', Path.DirectorySeparatorChar);
            var imagePath = Path.Combine(AppContext.BaseDirectory, relativePath);
            if (!relativePath.Contains(Path.DirectorySeparatorChar))
            {
                imagePath = Path.Combine(AppContext.BaseDirectory, "Image", relativePath);
            }

            return File.Exists(imagePath) ? imagePath : "/Image/picture.png";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenProductWindow(0);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductList.SelectedValue is not int productId)
            {
                MessageHelper.ShowInfo("Выберите товар для редактирования.");
                return;
            }

            OpenProductWindow(productId);
        }

        private void ProductList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsAdmin && ProductList.SelectedValue is int productId)
            {
                OpenProductWindow(productId);
            }
        }

        private void OpenProductWindow(int productId)
        {
            var window = new ProductWindow(productId);
            if (!window.IsReady)
            {
                return;
            }

            window.ShowDialog();
            LoadProducts();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductList.SelectedValue is not int productId)
            {
                MessageHelper.ShowInfo("Выберите товар для удаления.");
                return;
            }

            if (MessageBox.Show("Удалить выбранный товар?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using var dbContext = new AppDbContext();
                if (dbContext.OrderProducts.Any(p => p.IdProduct == productId))
                {
                    MessageHelper.ShowWarning("Товар есть в заказе. Сначала удалите его из заказа.");
                    return;
                }

                var product = dbContext.Products.Find(productId);
                if (product != null)
                {
                    dbContext.Products.Remove(product);
                    dbContext.SaveChanges();
                    LoadProducts();
                }
            }
            catch
            {
                MessageHelper.ShowError("Не удалось удалить товар. Проверьте подключение к базе данных.");
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}
