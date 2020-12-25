using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WordReview.Model;
using WordReview.Services;

namespace WordReview.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        #region Свойства
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
        private bool _IsBusy;
        public bool IsBusy
        {
            get => _IsBusy;
            set
            {
                Set(ref _IsBusy, _IsBusy = value);
                RaisePropertyChanged();
            }
        }
        private string _CurrentCount;
        public string CurrentCount
        {
            get => _CurrentCount;
            set
            {
                Set(ref _CurrentCount, _CurrentCount = value);
                RaisePropertyChanged();
            }
        }
        string _pattern;
        public string Pattern
        {
            get => _pattern;
            set
            {
                Set(ref _pattern, value);

                if (string.IsNullOrEmpty(Pattern))
                    Loaded();
                else
                    Find(Pattern);
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
        private ResponseCode _SelectedCode;
        public ResponseCode SelectedCode
        {
            get => _SelectedCode;
            set
            {
                Set(ref _SelectedCode, _SelectedCode = value);
                RaisePropertyChanged();
            }
        }
        int _dist;
        public int Dist
        {
            get => _dist;
            set
            {
                Set(ref _dist, value);
                if (!string.IsNullOrEmpty(Pattern))
                    Find(Pattern);
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
        public RelayCommand<IEnumerable<object>> EditAnswerAndAddCodeCommand => new RelayCommand<IEnumerable<object>>((items) => EditAnswerAndAddCode(items));
        public RelayCommand DeleteFileCommand => new RelayCommand(() => DeleteFile());
        #endregion
        public MainViewModel(IDataServices dataServices)
        {
            this.dataServices = dataServices;
            Loaded();
            Dist = 100;
        }
        private async void Find(string text)
        {
            GetAnswers.Clear();
            GetCodes.Clear();
            using (var uof = new UnitOfWork())
            {
                var values = new Dictionary<int, string>();
                foreach (var items in await uof.AnswersRepository.AsyncGetAll())                   
                    values.Add(items.Id, items.Answer);

               results = new List<Items>((await Fuzzy.GetAsyncListSearch(text, values)).OrderByDescending(p => p.Score).Where(p => p.Score > Dist));
                
                
                foreach (var x in results)
                    GetAnswers.Add((await uof.AnswersRepository.AsyncFindById(x.Key)));

                foreach (var item in await uof.ResponseCodeRepository.AsyncGet(p => p.Word.Contains(text)))
                    GetCodes.Add(item);
            }
        }

        private async void Loaded()
        {
            IsBusy = true;
            if (!File.Exists("DATA.db"))
                File.Create("DATA.db").Close();

            GetAnswers.Clear();
            GetCodes.Clear();
            using (var uof = new UnitOfWork())
            {
                foreach (var item in await uof.AnswersRepository.AsyncGetAll())
                    GetAnswers.Add(item);
                foreach (var item in await uof.ResponseCodeRepository.AsyncGetAll())
                    GetCodes.Add(item);
            }
            TotalCount = $"Загружено: {GetAnswers.Count}";
            CurrentCount = $"Обработано: {GetAnswers.Count(p => p.IsCheck == true)}";
            IsBusy = false;
        }
        private async void OpenFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.FilterIndex = 2;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == true)
            {
                IsBusy = true;
                await dataServices.AsyncLoaderFileToData(dialog.FileName);
                Loaded();
            }
            IsBusy = false;
        }
        private async void EditAnswerAndAddCode(IEnumerable<object> items)
        {
            var collection = new List<Answers>();
            collection.AddRange(items.Select(p => p as Answers));
            if (!GetCodes.Any())
            {
                var dialog = MessageBox.Show($"Добавить в кодировку значение: {Pattern}", "Сообщение", MessageBoxButton.YesNoCancel);
                switch (dialog)
                {
                    case MessageBoxResult.Yes:
                        using (var uof = new UnitOfWork())
                        {
                            await uof.ResponseCodeRepository.AsyncAdd(new ResponseCode { Word = Pattern });
                            uof.Commit();
                            GetCodes.Clear();
                            foreach (var item in await uof.ResponseCodeRepository.AsyncGet(p => p.Word.Contains(Pattern)))
                                GetCodes.Add(item);
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            foreach (var answer in collection)
            {
                IsBusy = true;
                char[] separatingStrings = { '.', ',', ';', ':', ' ' };
                using (var uof = new UnitOfWork())
                {
                    var temp = new List<string>();
                    var tmp = results.FirstOrDefault(p => p.Key == answer.Id);
                    if (tmp != null)
                    {
                        foreach (var it in tmp.Words.Where(p => p.Scope > Dist))
                            temp.Add(it.Text);
                        var array = answer.Answer.Split(separatingStrings);
                        array = array.Except(temp).ToArray();

                        answer.Answer = string.Join(" ", array);
                        answer.Code = string.Concat(answer.Code, $", {uof.ResponseCodeRepository.AsyncGet(p => p.Word.Contains(Pattern)).Result.FirstOrDefault().Id.ToString()}");
                        if (string.IsNullOrEmpty(answer.Answer))
                            answer.IsCheck = true;
                        await uof.AnswersRepository.AsyncUpdate(answer);
                        uof.Commit();
                    }
                }
            }
            IsBusy = false;
        }

        private async void DeleteFile()
        {
            IsBusy = true;
            GetAnswers.Clear();
            GetCodes.Clear();
            using (var uof = new UnitOfWork())
            {
                await uof.ResponseCodeRepository.AsyncRemoveRange(await uof.ResponseCodeRepository.AsyncGetAll());
                await uof.AnswersRepository.AsyncRemoveRange(await uof.AnswersRepository.AsyncGetAll());
                uof.Commit();
            }

            IsBusy = false;
        }
    }

    public static class Fuzzy
    {
        public static IEnumerable<Items> GetListSearch(string query, Dictionary<int,string> items)
        {
            var words = new List<Items>();
            foreach (var item in items)
                words.Add(new Items(item.Key, item.Value, query));
            return words;
        }
        public static Task<IEnumerable<Items>> GetAsyncListSearch(string query, Dictionary<int, string> items) =>
            Task.Run(() => GetListSearch(query, items));
    }

    public class Items
    {
        public int Key { get; private set; }
        public string Text { get; set; }
        public List<Word> Words { get; private set; } = new List<Word>();
        public int Score { get; private set; }
        public Items(int key, string text, string query)
        {
            Key = key;
            Text = text;
            var array = text.Split(separatingStrings);
            foreach (var item in array)
                Words.Add(new Word { Text = item, Scope = FuzzySharp.Fuzz.PartialRatio(query, item) });
            Score = Words.Max(p => p.Scope);
        }
        char[] separatingStrings = { '.', ',', ';', ':', ' ' };
    }
    public class Word
    {
        public string Text { get; set; }
        public int Scope { get; set; }
    }
}