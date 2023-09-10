using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Media;
using System.Threading.Tasks;
using ComicDownWpf.decoder;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NiL.JS.Expressions;
using System.Windows;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Security.Policy;
using System.Text.RegularExpressions;
using ComicDownWpf.pages;
using System.Windows.Documents;
using ComicPlugin;
using System.Windows.Forms;

namespace ComicDownWpf.viewmodel
{
    public delegate void CoverClickDelegate(DownTaskTree record);

    public class DownTaskTree : ObservableObject
    {
        private bool m_isChecked;
        private ObservableCollection<DownTaskRecord> m_chapterList;
        public delegate void DeleteNodeDelegate(DownTaskTree node);
        public event DeleteNodeDelegate DeleteNodeEvent;


        public DownTaskTree()
        {
            NodeList = new ObservableCollection<DownTaskRecord>();
            CoverClickCommnd = new RelayCommand(CoverClick);
        }

        public event CoverClickDelegate CoverClickEvent;
        public ICommand CoverClickCommnd { get; set; }

        public ObservableCollection<DownTaskRecord> NodeList
        {
            get => m_chapterList;
            set { SetProperty(ref m_chapterList, value); }
        }

        public string ImageUrl { get; set; }
        public string ComicName {get;set;}
        public string Href {get;set;}
        public string CodeName { get; set; }
        public string ComicInfoUrl { get; set; }

        public bool IsChecked
        {
            get => m_isChecked;
            set { SetProperty(ref m_isChecked, value); }
        }

        private void CoverClick()
        {
            CoverClickEvent?.Invoke(this);
        }

        public void InsertTaskRecord(DownTaskRecord record)
        {
            DownTaskRecord downTask = null;
            foreach (var i in NodeList)
            {
                if (i.CodeName == record.CodeName && i.ChapterName == record.ChapterName)
                {
                    downTask = i;
                    return;
                }
            }

            if (downTask == null)
            {
                NodeList.Insert(0, record);
                record.DeleteComicEvent += DeleteComicFunction;
            }
        }

        public void AddTaskRecord(CharpterItem item)
        {
            DownTaskRecord downTask = null;
            ComicInfoUrl = item.ComicInfoUrl;

            foreach (var i in NodeList)
            {
                if (i.ChapterName == item.CharpterName)
                {
                    downTask = i;
                    break;
                }
            }

            if (downTask == null)
            {
                downTask = new DownTaskRecord();
                downTask.ComicName = item.ComicName;
                downTask.ChapterName = item.CharpterName;
                downTask.ImageUrl = ImageUrl;
                downTask.ComicInfoUrl = item.ComicInfoUrl;
                downTask.CodeName = item.CodeName;
                downTask.Href = item.Href;
                downTask.DeleteComicEvent += DeleteComicFunction;
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    NodeList.Insert(0, downTask);
                }));
                
                downTask.Start();
            }
            else
            {

            }
           
        }

        private void DeleteComicFunction(DownTaskRecord record)
        {
            record.Stop();

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                NodeList.Remove(record);
                if (NodeList.Count == 0)
                {
                    DeleteNodeEvent?.Invoke(this);
                }
            }));
        }
    }


    public class DownLoadPageViewModel : ObservableObject
    {
        private Task m_task = null;
        private ObservableCollection<DownTaskRecord> m_downList;
        private Dictionary<string, DownTaskTree> m_treeDict;
        public ICommand FullCheckCommand { get; set; }
        public ICommand FullUnCheckCommand { get; set; }
        public ICommand DeletePatchCommand { get; set; }
        public event CoverClickDelegate CoverClickEvent;

        public DownLoadPageViewModel()
        {                    
            ShowList = new ObservableCollection<DownTaskTree>();
            m_treeDict = new Dictionary<string, DownTaskTree>();
            LoadDownLoadTask();
        }

        public ObservableCollection<DownTaskTree> ShowList { get; set; }

        public ObservableCollection<DownTaskRecord> DownTaskList 
        { 
            get { return m_downList; }
            set { m_downList = value; }
        }

        private void TaskNode_CoverClickEvent(DownTaskTree record)
        {
            CoverClickEvent?.Invoke(record);
        }

        private void LoadDownLoadTask()//从数据库读取
        {
            DownTaskList = ComicBookShelf.GetAllDownRecord();

            foreach (var i in DownTaskList)
            {
                DownTaskTree node = null;

                if(!m_treeDict.ContainsKey(i.ComicName))
                {
                    node = new DownTaskTree();
                    node.CoverClickEvent += TaskNode_CoverClickEvent;
                    m_treeDict.Add(i.ComicName, node);
                }
                else
                {
                    node = m_treeDict[i.ComicName];
                }

                node.InsertTaskRecord(i);
                node.ImageUrl = i.ImageUrl;
                node.ComicName = i.ComicName;
                node.CodeName = i.CodeName;
                node.Href = i.Href;
                node.ComicInfoUrl = i.ComicInfoUrl;
                node.DeleteNodeEvent += Node_DeleteNodeEvent;

                if(!ShowList.Contains(node))
                {
                    ShowList.Add(node);
                }
                
            }

        }

        private void Node_DeleteNodeEvent(DownTaskTree node)
        {
            ShowList?.Remove(node);
            if (m_treeDict.ContainsKey(node.ComicName))
            {
                m_treeDict.Remove(node.ComicName);
            } 
        }

        public void AddDownLoadTask(ObservableCollection<CharpterItem> list)
        {
            if(m_task == null || m_task.Status == TaskStatus.RanToCompletion)
            {
                m_task = new Task(new Action(() => {
                    if (list.Count > 0)
                    {
                        DownTaskTree taskNode = null;
                        var comicName = list[0].ComicName;
                        var codeName = list[0].CodeName;

                        foreach (var i in ShowList)
                        {
                            if (i.ComicName == comicName && i.CodeName == codeName)
                            {
                                taskNode = i;
                            }
                        }

                        if (taskNode == null)
                        {
                            taskNode = new DownTaskTree();
                            taskNode.ComicName = list[0].ComicName;
                            taskNode.CodeName = list[0].CodeName;
                            taskNode.ImageUrl = CommonManager.CacheImage(list[0].ImageUrl, list[0].CodeName);
                            taskNode.Href = list[0].Href;
                            taskNode.CoverClickEvent += TaskNode_CoverClickEvent;
                            taskNode.DeleteNodeEvent += Node_DeleteNodeEvent;
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                ShowList.Insert(0, taskNode);
                            }));
                            
                        }

                        foreach (var i in list)
                        {
                            taskNode.AddTaskRecord(i);
                        }

                    }
                }));
                m_task.Start();
            }         
            
        }

 
    }

    public class DownLoadBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = (bool)value;

            if (ret)
            {
                return Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
