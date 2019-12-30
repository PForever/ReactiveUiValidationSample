using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using ReactiveUI.Validation.States;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.ComponentModel;
using ReactiveUI.Validation.Collections;

namespace ReactiveUiValidationSample
{
    public class MainWindowVm : ReactiveValidationObject<MainWindowVm>
    {
        public ObservableCollection<ErrInfo> Errors { get; } = new ObservableCollection<ErrInfo>(/*new[] { new ErrInfo("Test", "Test1") }*/);
        public string LastName { get => _lastName; set => this.RaiseAndSetIfChanged(ref _lastName, value); }
        private string _firstName;
        private string _lastName;

        public string FirstName { get => _firstName; set => this.RaiseAndSetIfChanged(ref _firstName, value); }
        public IReactiveCommand Save { get; set; }
        public MainWindowVm()
        {
            this.ValidationRule(
                    x => x.FirstName,
                    name => !string.IsNullOrWhiteSpace(name),
                    "First Name shouldn't be empty.");
            this.ValidationRule(
                    x => x.LastName,
                    name => !string.IsNullOrWhiteSpace(name),
                    "Last Name shouldn't be empty.");
            var isValid = this.IsValid();
            Save = ReactiveCommand.Create(() => System.Windows.Forms.MessageBox.Show($"{LastName} {FirstName}"), isValid);
            //var errorChanged = Observable.FromEventPattern<DataErrorsChangedEventArgs>(this, nameof(ErrorsChanged)).Select(e => e.EventArgs.PropertyName).Subscribe(p => ResetErrorsList(p));
            this.ValidationContext.ValidationStatusChange/*.Throttle(TimeSpan.FromMilliseconds(30))*/.Select(e => e.Text).DistinctUntilChanged().Subscribe(ResetErrorsList);

            //ErrorsChanged += (o, e) => ResetErrorsList(e.PropertyName);

        }
        private void ResetErrorsList(ValidationText text)
        {
            
            var errors = Errors;
            errors.Clear();
            foreach (var item in text)
            {
                errors.Add(new ErrInfo("", item));
            }
        }
        private void ResetErrorsList(string propertyName)
        {
            var errors = Errors;
            var errMessage = GetErrors(propertyName).Cast<object>().Select(er => er.ToString()).ToArray();
            if (errMessage.Length == 0)
            {
                errors.Clear();
                return;
            }
            var errs = new ErrInfo(propertyName, string.Join(", ", errMessage));
            if (errors.Select((er, i) => (Err: er, Index: i)).FirstOrDefault(er => er.Err.PropertyName == propertyName) is var oldErr)
            {
                errors.RemoveAt(oldErr.Index);
                errors.Insert(oldErr.Index, errs);
                return;
            }
            errors.Add(errs);

        }

        private void value(object sender, DataErrorsChangedEventArgs e) => throw new NotImplementedException();
        private static bool NewMethod(string n) => !string.IsNullOrEmpty(n);
    }
    public struct ErrInfo
    {
        public string PropertyName { get; set; }
        public string Message { get; set; }

        public ErrInfo(string propertyName, string message)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
