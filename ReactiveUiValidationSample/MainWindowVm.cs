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
using DynamicData;
using DynamicData.Binding;

namespace ReactiveUiValidationSample
{
    public class MainWindowVm : ReactiveValidationObject<MainWindowVm>
    {
        private ReadOnlyObservableCollection<Parent> _parents;

        public class Parent : ReactiveValidationObject<Parent>
        {
            [Reactive] public string FirstName { get; set; }
            [Reactive] public string LastName { get; set; }
            public Parent()
            {

            }
            public Parent(string firstName, string lastName)
            {
                FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
                LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
                SetValidationRules();
            }
            private void SetValidationRules()
            {
                this.ValidationRule(
                                    x => x.FirstName,
                                    name => !string.IsNullOrWhiteSpace(name),
                                    "First Name shouldn't be empty.");
                this.ValidationRule(
                                    x => x.LastName,
                                    name => !string.IsNullOrWhiteSpace(name),
                                    "Last Name shouldn't be empty.");
            }
        }
        [Reactive] public int Age { get; set; }
        [Reactive] public bool Sex { get; set; }
        [Reactive] public string LastName { get; set; }
        [Reactive] public string FirstName { get; set; }
        //[Reactive] public IObservableList<Parent> Parents { get; set; }
        public ReadOnlyObservableCollection<Parent> Parents { get => _parents; }
        //public extern Parent Parents { [ObservableAsProperty]get; }
        public ObservableCollection<ErrInfo> Errors { get; } = new ObservableCollection<ErrInfo>();
        //public string LastName { get => _lastName; set => this.RaiseAndSetIfChanged(ref _lastName, value); }
        public IReactiveCommand Save { get; set; }
        public IReactiveCommand Add { get; set; }
        private SourceList<Parent> _parentSources;
        public IChangeSet<Parent> ParentChangeSet { [ObservableAsProperty]get; }
        public MainWindowVm()
        {
            SetValidationRules();
            var list = _parentSources = new SourceList<Parent>();
            list.AddRange(new[] { new Parent("Qwe", "Ewq"), new Parent("Qwe2", "Ewq2") });
            list.Connect().ObserveOn(RxApp.MainThreadScheduler).Bind(out _parents).ToPropertyEx(this, x => x.ParentChangeSet);
            var isValid = this.IsValid();
            Save = ReactiveCommand.Create(() => System.Windows.Forms.MessageBox.Show($"{LastName} {FirstName}"), isValid);
            Add = ReactiveCommand.Create(() => list.Add(new Parent("", "")), outputScheduler: RxApp.MainThreadScheduler);
            this.ValidationContext.ValidationStatusChange.Throttle(TimeSpan.FromMilliseconds(30)).ObserveOn(RxApp.MainThreadScheduler).Select(e => e.Text).DistinctUntilChanged().Subscribe(ResetErrorsList);

            //ErrorsChanged += (o, e) => ResetErrorsList(e.PropertyName);

        }

        private void SetValidationRules()
        {
            this.ValidationRule(
                                x => x.FirstName,
                                name => !string.IsNullOrWhiteSpace(name),
                                "First Name shouldn't be empty.");
            this.ValidationRule(
                                x => x.LastName,
                                name => !string.IsNullOrWhiteSpace(name),
                                "Last Name shouldn't be empty.");
            this.ValidationRule(
                                x => x.Age,
                                age => age > 0,
                                age => $"Age {age} is uncorrect. Must be grate then 0");

            var sexAndAge = this.WhenAnyValue(x => x.Sex, x => x.FirstName, (sex, name) => (Sex: sex, Name: name)).Select(sn => sn.Sex && sn.Name != "Ольга" || !sn.Sex);
            this.ValidationRule(_ => sexAndAge, (sn, state) => state ? "" : "Ольга не мужское имя");
            var changeSet = this.WhenAnyValue(x => x.ParentChangeSet).Select(set => set.Where(s => s.Reason == ListChangeReason.Add || s.Reason == ListChangeReason.AddRange || s.Reason == ListChangeReason.Refresh).All(s => s.Item.Current.IsValid()));
        }

        private void ResetErrorsList(ValidationText text)
        {

            var errors = Errors;
            errors.Clear();
            foreach (var item in text)
            {
                errors.Add(new ErrInfo(item));
            }
        }
    }
    public struct ErrInfo
    {
        public string Message { get; set; }

        public ErrInfo(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
