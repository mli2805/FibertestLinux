using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace WpfCommonViews
{
    public class MyMessageBoxViewModel : Screen
    {
        private readonly string _caption;
        public List<MyMessageBoxLineModel> Lines { get; set; }
        public Visibility OkVisibility { get; set; }
        public Visibility CancelVisibility { get; set; }
        public bool IsAnswerPositive { get; set; }


        public MyMessageBoxViewModel(MessageType messageType, string message)
        {
            Lines = new List<MyMessageBoxLineModel>()
            {
                new MyMessageBoxLineModel(){Line = message, FontWeight = FontWeights.Bold}
            };

            _caption = messageType.GetLocalizedString();
            OkVisibility = messageType.ShouldOkBeVisible();
            CancelVisibility = messageType.ShouldCancelBeVisible();
            IsAnswerPositive = false;
        }

        public MyMessageBoxViewModel(MessageType messageType, IEnumerable<string> strs, int focusedString = Int32.MaxValue)
        {
            Lines = strs.Select(s => new MyMessageBoxLineModel() {Line = s}).ToList();
            if (focusedString == -1)
                Lines.ForEach(l=>l.FontWeight = FontWeights.Bold);
            else if (focusedString < Lines.Count)
                Lines[focusedString].FontWeight = FontWeights.Bold;

            _caption = messageType.GetLocalizedString();
            OkVisibility = messageType.ShouldOkBeVisible();
            CancelVisibility = messageType.ShouldCancelBeVisible();
            IsAnswerPositive = false;
        }

        public MyMessageBoxViewModel(MessageType messageType, IEnumerable<MyMessageBoxLineModel> lines)
        {
            Lines = lines.ToList();

            _caption = messageType.GetLocalizedString();
            OkVisibility = messageType.ShouldOkBeVisible();
            CancelVisibility = messageType.ShouldCancelBeVisible();
            IsAnswerPositive = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }
        public async void OkButton()
        {
            IsAnswerPositive = true;
            await TryCloseAsync();
        }

        public async void CancelButton()
        {
            IsAnswerPositive = false;
            await TryCloseAsync();
        }

        /// <summary>Just for debug purposes </summary>
        [Localizable(false)]
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "MyMessageBoxViewModel:" + Lines.FirstOrDefault();
        }
    }
}
