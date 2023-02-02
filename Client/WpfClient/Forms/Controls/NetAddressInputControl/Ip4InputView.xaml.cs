using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Fibertest.WpfClient
{
    /// <summary>
    /// Interaction logic for Ip4InputView.xaml
    /// </summary>
    public partial class Ip4InputView
    {
        public Ip4InputView()
        {
            InitializeComponent();
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void TextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == @"." || e.Text == @",")
            {
                MoveFocus();
                e.Handled = true;
            }
            else
            {
                var textbox = (TextBox)sender;
                var caret = textbox.CaretIndex;
                var selectionStart = textbox.SelectionStart;
                var selectionLength = textbox.SelectionLength;
                var textWithoutSelected = textbox.Text.Substring(0, selectionStart) +
                                          textbox.Text.Substring(selectionStart + selectionLength);
                var resultingText = textWithoutSelected.Substring(0, caret) + e.Text + textWithoutSelected.Substring(caret);
                e.Handled = !IsInRange(resultingText);
            }
        }

        private bool IsInRange(string text)
        {
            int number;
            if (!int.TryParse(text, out number))
                return false;
            return number >= 0 && number < 256;
        }

        private void StackPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MoveFocus();

                e.Handled = true;
            }
        }

        private static void MoveFocus()
        {
            TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
            UIElement? keyboardFocus = Keyboard.FocusedElement as UIElement;
            keyboardFocus?.MoveFocus(tRequest);
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            var text = ((TextBox)e.Source).Text;
            if (int.TryParse(text, out int number))
                ((TextBox)e.Source).Text = number.ToString();
        }

    }
}
