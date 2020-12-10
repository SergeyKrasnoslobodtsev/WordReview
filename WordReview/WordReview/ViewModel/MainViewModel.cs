using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using WordReview.Model;
using WordReview.Services;

namespace WordReview.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        private string _TotalCount;
        public string TotalCount
        {
            get => _TotalCount;
            set
            {
                Set(ref _TotalCount, _TotalCount = value);
                RaisePropertyChanged();
            }
        }
        private Answers _SelectedItem;
        public Answers SelectedItem
        {
            get => _SelectedItem;
            set
            {
                Set(ref _SelectedItem, _SelectedItem = value);
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<Answers> GetAnswers { get; set; } = new ObservableCollection<Answers>();
        public ObservableCollection<Answers> GetSearchAnswers { get; set; } = new ObservableCollection<Answers>();
        private IDataServices dataServices;
        public RelayCommand OpenFileCommand => new RelayCommand(() => OpenFile());
        public RelayCommand<string> FindCommand => new RelayCommand<string>((arg) => Find(arg));
        public MainViewModel(IDataServices dataServices)
        {
            this.dataServices = dataServices;
            Loaded();
        }
        private void Find(string text)
        {
            GetSearchAnswers.Clear();
            var results = FuzzySharp.Process.ExtractAll(text, GetAnswers.Select(p => p.Answer), (s) => s);
            foreach (var x in results)
            {
                if (x.Score > 50)
                {
                    GetSearchAnswers.Add(GetAnswers.ElementAt(x.Index));
                }
            }
        }

        private async void Loaded()
        {
            using (var uof = new UnitOfWork.UnitOfWork())
            {
                foreach (var item in await uof.AnswersRepository.AsyncGetAll())
                    GetAnswers.Add(item);
            }
            TotalCount = $"Загружено: {GetAnswers.Count}";
        }
        private async void OpenFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.FilterIndex = 2;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == true)
            {
                foreach (var item in await dataServices.GetAsyncLoadAnswers(dialog.FileName))
                    GetAnswers.Add(item);
                TotalCount = $"Загружено: {GetAnswers.Count}";
            }
        }
    }
}