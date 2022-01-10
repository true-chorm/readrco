using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

using readrco.src.model;
using readrco.src.tool;
using readrco.src.xml;

namespace readrco
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string TAG = "readrco";

		private NewRecord newRecordWindow;

		public MainWindow()
		{
			InitializeComponent();
			Logger.v(TAG, "hello world");

			if(XMLManager.Init())
			{
				if(XMLManager.LoadRecord())
				{
					Logger.v(TAG, "Load record finished, count:" + XMLManager.GetRecords().Count);
					foreach(Record rco in XMLManager.GetRecords())
					{
						Logger.v(TAG, rco.ToString());
						LVList.Items.Add(rco);
					}
				}
				else
				{
					MessageBox.Show("读取记录时发生错误");
					Application.Current.Shutdown();
				}
			}
			else
			{
				MessageBox.Show("无法初始化读书记录数据文件");
				Application.Current.Shutdown();
			}

			LVList.MouseDoubleClick += LVList_MouseDoubleClick;
		}

		private void LVList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if(e.ChangedButton == MouseButton.Left)
			{
				Btn_Edit_Click(sender, e);
			}
		}

		private void Btn_New_Click(object sender, RoutedEventArgs e)
		{
			if(newRecordWindow != null)
			{
				newRecordWindow.Closed -= RecordEditWindowClosed;
				newRecordWindow.Close();
			}

			newRecordWindow = new NewRecord(null, GetCurMaxID());
			newRecordWindow.Closed += RecordEditWindowClosed;
			newRecordWindow.Show();
		}

		private void Btn_Edit_Click(object sender, RoutedEventArgs e)
		{
			if(LVList.SelectedItem is Record rco)
			{
				Logger.v(TAG, "Editing the record of " + rco.Book.MainTitle);
				if(newRecordWindow != null)
				{
					newRecordWindow.Closed -= RecordEditWindowClosed;
					newRecordWindow.Close();
				}

				newRecordWindow = new NewRecord(rco, GetCurMaxID());
				newRecordWindow.Closed += RecordEditWindowClosed;
				newRecordWindow.Show();
			}
		}

		private int GetCurMaxID()
		{
			int max = 0;
			List<Record> records = XMLManager.GetRecords();
			for(int i = 0; i < records.Count; i++)
			{
				if(max < records[i].ID)
					max = records[i].ID;
			}

			return max;
		}

		private void RecordEditWindowClosed(object? sender, EventArgs e)
		{
			if(newRecordWindow.rcoStoraged != null)
			{
				if(LVList.Items.Count > 0)
				{
					if(newRecordWindow.rcoStoraged.ID > ((Record)LVList.Items[0]).ID)
					{
						XMLManager.InsertRecord(0, newRecordWindow.rcoStoraged);
						LVList.Items.Insert(0, newRecordWindow.rcoStoraged);
					}
				}
				else
				{
					XMLManager.InsertRecord(0, newRecordWindow.rcoStoraged);
					LVList.Items.Insert(0, newRecordWindow.rcoStoraged);
				}
			}

			newRecordWindow = null;
		}
	}
}
