using System.Windows;

namespace DemoExam2.Helpers
{
    public static class MessageHelper
    {
        public static void ShowInfo(string text)
        {
            MessageBox.Show(text, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowWarning(string text)
        {
            MessageBox.Show(text, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void ShowError(string text)
        {
            MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
