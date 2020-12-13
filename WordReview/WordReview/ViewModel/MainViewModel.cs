using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WordReview.Model;
using WordReview.Services;
using System.Web;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WordReview.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        #region Свойства
        private string _TotalCount;
        public string TotalCount {
            get => _TotalCount;
            set {
                Set(ref _TotalCount, _TotalCount = value);
                RaisePropertyChanged();
            }
        }
        private string _CurrentCount;
        public string CurrentCount {
            get => _CurrentCount;
            set {
                Set(ref _CurrentCount, _CurrentCount = value);
                RaisePropertyChanged();
            }
        }
        string _pattern;
        public string Pattern {
            get => _pattern;
            set {
                Set(ref _pattern, value);

                if (string.IsNullOrEmpty(Pattern))
                    Loaded();
                else
                    Find(Pattern);
            }
        }

        private Answers _SelectedItem;
        public Answers SelectedItem {
            get => _SelectedItem;
            set {
                Set(ref _SelectedItem, _SelectedItem = value);
                RaisePropertyChanged();
            }
        }
        private ResponseCode _SelectedCode;
        public ResponseCode SelectedCode {
            get => _SelectedCode;
            set {
                Set(ref _SelectedCode, _SelectedCode = value);
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Коллекции
        public ObservableCollection<Answers> GetAnswers { get; set; } = new ObservableCollection<Answers>();
        public ObservableCollection<ResponseCode> GetCodes { get; set; } = new ObservableCollection<ResponseCode>();
        #endregion

        private IDataServices dataServices;
        private List<Items> results = null;
        #region Команды
        public RelayCommand OpenFileCommand => new RelayCommand(() => OpenFile());
        public RelayCommand<string> FindCommand => new RelayCommand<string>((arg) => Find(arg));
        public RelayCommand EditAnswerAndAddCodeCommand => new RelayCommand(() => EditAnswerAndAddCode());
        #endregion
        public MainViewModel(IDataServices dataServices) {
            this.dataServices = dataServices;
            Loaded();
        }
        private async void Find(string text) { 
            GetAnswers.Clear();

            using (var uof = new UnitOfWork.UnitOfWork()) {
                results = new List<Items>((await Fuzzy.GetAsyncListSearch(text, uof.AnswersRepository.GetAll().Select(p => p.Answer))).OrderByDescending(p => p.Score).Where(p => p.Score > 50)); //FuzzySharp.Process.ExtractAll(text, uof.AnswersRepository.GetAll().Select(p => p.Answer), (s) => s).OrderByDescending(p => p.Score).Where(p => p.Score > 50);
                foreach (var x in results)
                    GetAnswers.Add(await uof.AnswersRepository.AsyncFindById(x.Key));
            }
        }

        private async void Loaded() {
            GetAnswers.Clear();
            using (var uof = new UnitOfWork.UnitOfWork()) {
                foreach (var item in await uof.AnswersRepository.AsyncGetAll())
                    GetAnswers.Add(item);
            }
            TotalCount = $"Загружено: {GetAnswers.Count}";
            CurrentCount = $"Обработано: {GetAnswers.Count(p =>p.IsCheck == true)}";
        }
        private async void OpenFile() {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.FilterIndex = 2;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == true) {
                await dataServices.AsyncLoaderFileToData(dialog.FileName);
                Loaded();
            }
        }
        private async void EditAnswerAndAddCode() {

            using (var uof = new UnitOfWork.UnitOfWork()) {
                var temp = new List<string>();
                var tmp = results.FirstOrDefault(p => p.Key == SelectedItem.Id);//.Words.Where(p => p.Scope > 50).Select(p => p.Text);
                foreach (var it in tmp.Words.Where(p => p.Scope > 50))
                    temp.Add(it.Text);
                var array = SelectedItem.Answer.Split(' ');
                array = array.Except(temp).ToArray();

                SelectedItem.Answer = string.Join(" ", array);
                if (string.IsNullOrEmpty(SelectedItem.Answer))
                    SelectedItem.IsCheck = true;
                await uof.AnswersRepository.AsyncUpdate(SelectedItem);
            }
            
        }
    }

    public static class Fuzzy {
        public static IEnumerable<Items> GetListSearch(string query, IEnumerable<string> items) {
            var words = new List<Items>();
            int Key = 1;
            foreach (var item in items)
                words.Add(new Items(Key++, item, query));
            return words;
        }
        public static Task<IEnumerable<Items>> GetAsyncListSearch(string query, IEnumerable<string> items) =>
            Task.Run(() => GetListSearch(query, items));
    }

    public class Items
    {
        public int Key { get; private set; }
        public string Text { get; set; }
        public List<Word> Words { get; private set; } = new List<Word>();
        public int Score { get; private set; }
        public Items(int key, string text, string query) {
            Key = key;
            Text = text;
            var array = text.Split(' ');
            foreach (var item in array)
                Words.Add(new Word { Text = item, Scope = FuzzySharp.Fuzz.PartialRatio(query, item) });
            Score = Words.Max(p => p.Scope);
        }
    }
    public class Word
    {
        public string Text { get; set; }
        public int Scope { get; set; }
    }
}