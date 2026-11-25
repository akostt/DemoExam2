using DemoExam2.Data;
using DemoExam2.Helpers;
using DemoExam2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DemoExam2
{
    public partial class ProductWindow : Window
    {
        private readonly int _productId;
        private string? _selectedPhoto;
        public bool IsReady { get; private set; } = true;

        public ProductWindow(int productId)
        {
            _productId = productId;
            InitializeComponent();
            Title = productId == 0 ? "Добавление товара" : "Редактирование товара";
            IdLabel.Visibility = productId == 0 ? Visibility.Collapsed : Visibility.Visible;
            IdBox.Visibility = IdLabel.Visibility;

            try
            {
                using var dbContext = new AppDbContext();
                CategoryBox.ItemsSource = dbContext.Categories.OrderBy(c => c.Name).ToList();
                ManufacturerBox.ItemsSource = dbContext.Manufacturers.OrderBy(m => m.Name).ToList();
                FactoryBox.ItemsSource = dbContext.Factories.OrderBy(f => f.Name).ToList();
                UnitBox.ItemsSource = dbContext.Units.OrderBy(u => u.Name).ToList();

                if (productId > 0)
                {
                    var product = dbContext.Products.FirstOrDefault(p => p.Id == productId);
                    if (product == null)
                    {
                        MessageHelper.ShowError("Товар не найден. Закройте форму и обновите список.");
                        IsReady = false;
                        return;
                    }

                    LoadProduct(product);
                }
            }
            catch
            {
                MessageHelper.ShowError("Не удалось загрузить данные формы. Проверьте подключение к базе данных.");
                IsReady = false;
            }
        }

        private void LoadProduct(Product product)
        {
            IdBox.Text = product.Id.ToString();
            ArticulBox.Text = product.Articul;
            NameBox.Text = product.Name;
            CategoryBox.SelectedValue = product.IdCategory;
            DescriptionBox.Text = product.Description;
            ManufacturerBox.SelectedValue = product.IdManufacturer;
            FactoryBox.SelectedValue = product.IdFactory;
            PriceBox.Text = product.Price.ToString("0.00");
            UnitBox.SelectedValue = product.IdUnit;
            CountBox.Text = product.Count.ToString();
            DiscountBox.Text = product.Discount.ToString();
            PhotoName.Text = string.IsNullOrEmpty(product.Image) ? "Фото не выбрано" : product.Image;

            var relativePath = (product.Image ?? string.Empty).Replace('/', Path.DirectorySeparatorChar);
            var photoPath = Path.Combine(AppContext.BaseDirectory, relativePath);
            if (!relativePath.Contains(Path.DirectorySeparatorChar))
            {
                photoPath = Path.Combine(AppContext.BaseDirectory, "Image", relativePath);
            }

            if (File.Exists(photoPath))
            {
                PhotoPreview.Source = new BitmapImage(new Uri(photoPath));
            }
        }

        private void ChoosePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var selectedPhoto = dialog.FileName;
                    var photo = new BitmapImage();
                    photo.BeginInit();
                    photo.CacheOption = BitmapCacheOption.OnLoad;
                    photo.UriSource = new Uri(selectedPhoto);
                    photo.EndInit();

                    _selectedPhoto = selectedPhoto;
                    PhotoName.Text = Path.GetFileName(selectedPhoto);
                    PhotoPreview.Source = photo;
                }
                catch
                {
                    MessageHelper.ShowError("Не удалось открыть фото. Выберите другой файл изображения.");
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ArticulBox.Text) || string.IsNullOrWhiteSpace(NameBox.Text)
                || CategoryBox.SelectedValue == null || ManufacturerBox.SelectedValue == null
                || FactoryBox.SelectedValue == null || UnitBox.SelectedValue == null)
            {
                MessageHelper.ShowWarning("Заполните артикул, наименование и выберите категорию, производителя, поставщика и единицу измерения.");
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out var price) || price < 0 || price != decimal.Round(price, 2))
            {
                MessageHelper.ShowWarning("Введите цену не меньше 0, не более двух знаков после запятой.");
                return;
            }

            if (!int.TryParse(CountBox.Text, out var count) || count < 0)
            {
                MessageHelper.ShowWarning("Количество на складе должно быть целым числом не меньше 0.");
                return;
            }

            if (!int.TryParse(DiscountBox.Text, out var discount) || discount < 0 || discount > 100)
            {
                MessageHelper.ShowWarning("Скидка должна быть целым числом от 0 до 100.");
                return;
            }

            try
            {
                using var dbContext = new AppDbContext();
                var articul = ArticulBox.Text.Trim();
                if (dbContext.Products.Any(p => p.Articul == articul && p.Id != _productId))
                {
                    MessageHelper.ShowWarning("Товар с таким артикулом уже есть. Укажите другой артикул.");
                    return;
                }

                var product = _productId == 0
                    ? new Product
                    {
                        Id = (dbContext.Products.Select(p => (int?)p.Id).Max() ?? 0) + 1,
                        Image = string.Empty
                    }
                    : dbContext.Products.FirstOrDefault(p => p.Id == _productId);

                if (product == null)
                {
                    MessageHelper.ShowError("Товар не найден. Закройте форму и обновите список.");
                    return;
                }

                var oldImage = product.Image ?? string.Empty;
                product.Articul = articul;
                product.Name = NameBox.Text.Trim();
                product.IdCategory = (int)CategoryBox.SelectedValue;
                product.Description = DescriptionBox.Text.Trim();
                product.IdManufacturer = (int)ManufacturerBox.SelectedValue;
                product.IdFactory = (int)FactoryBox.SelectedValue;
                product.Price = price;
                product.IdUnit = (int)UnitBox.SelectedValue;
                product.Count = count;
                product.Discount = discount;
                if (_selectedPhoto != null)
                {
                    product.Image = SavePhoto(_selectedPhoto);
                }
                else if (_productId == 0)
                {
                    product.Image = string.Empty;
                }

                if (_productId == 0)
                {
                    dbContext.Products.Add(product);
                }

                dbContext.SaveChanges();
                RemoveOldPhoto(dbContext, product.Id, oldImage, product.Image);
                DialogResult = true;
            }
            catch
            {
                MessageHelper.ShowError("Не удалось сохранить товар. Проверьте данные и подключение к базе данных.");
            }
        }

        private string SavePhoto(string path)
        {
            var fileName = $"product_{Guid.NewGuid():N}{Path.GetExtension(path)}";
            var imageFolder = Path.Combine(AppContext.BaseDirectory, "Image");
            Directory.CreateDirectory(imageFolder);
            File.Copy(path, Path.Combine(imageFolder, fileName));
            return $"Image/{fileName}";
        }

        private void RemoveOldPhoto(AppDbContext dbContext, int productId, string? oldImage, string newImage)
        {
            if (string.IsNullOrEmpty(oldImage) || oldImage == newImage
                || oldImage.EndsWith("picture.png", StringComparison.OrdinalIgnoreCase)
                || dbContext.Products.Any(p => p.Id != productId && p.Image == oldImage))
            {
                return;
            }

            var photoPath = Path.Combine(AppContext.BaseDirectory, "Image", Path.GetFileName(oldImage));
            if (File.Exists(photoPath))
            {
                File.Delete(photoPath);
            }
        }
    }
}
