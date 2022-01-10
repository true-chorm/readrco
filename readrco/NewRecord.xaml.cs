using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using readrco.src.model;
using readrco.src.tool;
using readrco.src.xml;

namespace readrco
{
	/// <summary>
	/// Interaction logic for NewRecord.xaml
	/// </summary>
	public partial class NewRecord : Window
	{
		private const string TAG = "NewRecord";

		private readonly int curMaxID;
		private readonly Record record;
		internal Record rcoStoraged;

		public NewRecord(Record record, int curmax)
		{
			this.record = record;
			curMaxID = curmax;
			InitializeComponent();
		}

		private void Read_Status_Watcher(object sender, RoutedEventArgs e)
		{
			if(sender == RBReading)
			{
				GBStar.Visibility = Visibility.Collapsed;
			}
			else if(sender == RBRead)
			{
				GBStar.Visibility = Visibility.Visible;
			}
		}

		private void Add_Author(object sender, RoutedEventArgs e)
		{
			SPAuthors.Children.Add(GenRemovableTextBox(SPAuthors));
		}

		private void Add_Translator(object sender, RoutedEventArgs e)
		{
			SPTranslators.Children.Add(GenRemovableTextBox(SPTranslators));
		}

		private void Remove_UIElement_Click(object sender, MouseButtonEventArgs e)
		{
			if(sender is Image)
			{
				Image img = sender as Image;
				StackPanel parent = (StackPanel)VisualTreeHelper.GetParent(img);
				StackPanel pparent = (StackPanel)img.Tag;
				pparent.Children.Remove(parent);
			}
		}

		private void HighFrqCharacter_Click(object sender, MouseButtonEventArgs e)
		{
			if(sender is TextBlock)
			{
				TextBlock tb = sender as TextBlock;
				TextBox tbox = GetFocusedTextBox();
				if(tbox != null)
				{
					tbox.AppendText(tb.Text.Trim());
					tbox.SelectionStart = tbox.Text.Length;
				}
			}
		}

		private StackPanel GenRemovableTextBox(StackPanel parent)
		{
			StackPanel panel = new StackPanel();
			panel.Orientation = Orientation.Horizontal;
			panel.Margin = new Thickness
			{
				Top = 2
			};

			TextBox tbox = new TextBox
			{
				Style = (Style)Resources["NarrowTextBoxWidth"]
			};

			BitmapImage bimg = new BitmapImage();
			bimg.BeginInit();
			bimg.UriSource = new Uri(@"res/minus.png", UriKind.Relative);
			bimg.EndInit();
			Image img = new Image
			{
				Width = 18,
				Margin = new Thickness
				{
					Left = 5
				},
				Source = bimg
			};
			img.MouseDown += Remove_UIElement_Click;
			img.Tag = parent;
			
			panel.Children.Add(tbox);
			panel.Children.Add(img);

			return panel;
		}

		private TextBox GetFocusedTextBox()
		{
			if(TBMainTitle.IsFocused)
				return TBMainTitle;
			else if(TBSubTitle.IsFocused)
				return TBSubTitle;
			else if(TBPublish.IsFocused)
				return TBPublish;
			else if(TBPubSn.IsFocused)
				return TBPubSn;
			else if(TBBeginDate.IsFocused)
				return TBBeginDate;
			else if(TBEndDate.IsFocused)
				return TBEndDate;
			else
			{
				UIElementCollection children = SPAuthors.Children;
				TextBox tbox;
				foreach(UIElement sp in children)
				{
					tbox = FindFocusedTextBoxInAuthorsAndTranslaters(sp);
					if(tbox != null)
						return tbox;
				}

				children = SPTranslators.Children;
				foreach(UIElement sp in children)
				{
					tbox = FindFocusedTextBoxInAuthorsAndTranslaters(sp);
					if(tbox != null)
						return tbox;
				}
			}

			return null;
		}

		private TextBox FindFocusedTextBoxInAuthorsAndTranslaters(UIElement sp)
		{
			StackPanel spanel;
			if(sp is StackPanel)
			{
				spanel = sp as StackPanel;
				if(spanel.Children.Count is 2)
				{
					if(spanel.Children[0].IsFocused)
					{
						if(spanel.Children[0] is TextBox)
						{
							return (TextBox)spanel.Children[0];
						}
					}
				}
			}

			return null;
		}

		private void Today_Click(object sender, RoutedEventArgs e)
		{
			if(sender == TBBD)
			{
				TBBeginDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
			}
			else if(sender == TBED)
			{
				TBEndDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
			}
		}

		private void Save_Btn_Click(object sender, RoutedEventArgs e)
		{
			Logger.v(TAG, "Save_Btn_Click()");
			if(TBMainTitle.Text.Length == 0)
			{
				MessageBox.Show("请输入图书主标题");
				return;
			}

			(string[] authors, byte author_count) = GetAuthors();
			Logger.v(TAG, "author_count:" + author_count);
			if(author_count == 0)
			{
				MessageBox.Show("请输入作者名称");
				return;
			}

			if(TBBeginDate.Text.Length == 0)
			{
				MessageBox.Show("请输入起阅日期");
				return;
			}

			if(RBRead.IsChecked is null || RBReading.IsChecked is null)
			{
				MessageBox.Show("请选择阅读状态");
				return;
			}
			bool reading = RBReading.IsChecked.Value;

			if(reading && TBEndDate.Text.Length > 0)
			{
				MessageBox.Show("终读日期与阅读状态冲突");
				return;
			}
			else if(!reading && TBEndDate.Text.Length == 0)
			{
				MessageBox.Show("请输入终读日期");
				return;
			}

			int sidx = CBStar.SelectedIndex;
			if(!reading)
			{
				if(sidx == -1)
				{
					MessageBox.Show("请选择评分");
					return;
				}
			}

			if(TBWords.Text.Length > 0)
			{
				char[] words = TBWords.Text.ToCharArray();
				foreach(char word in words)
				{
					if(word < '0' || word > '9')
					{
						if(word != '.')
						{
							MessageBox.Show("\"字数\"只能输入数字");
							return;
						}
					}
				}
			}

			(string[] translators, byte translator_count) = GetTranslators();
			Logger.v(TAG, "translator_count:" + translator_count);

			Record rco;
			Book book;
			if(this.record is null)
			{
				rco = new Record();
				rco.ID = curMaxID + 1;
				book = new Book();
				rco.Book = book;
			}
			else
			{
				rco = this.record;
				book = this.record.Book;
				book.ClearAuthors();
				book.ClearTranslators();
			}

			if(reading)
			{
				rco.Status = Record.STATUS_READING;
				rco.Star = null;
				rco.EndDate = "";
				rco.Comment = "";
			}
			else
			{
				rco.Status = Record.STATUS_READ;
				rco.Star = (byte)(CBStar.SelectedIndex + 1);
				rco.EndDate = TBEndDate.Text;
				rco.Comment = TBComment.Text;
			}
			
			rco.BeginDate = TBBeginDate.Text;
			book.MainTitle = TBMainTitle.Text;
			book.SubTitle = TBSubTitle.Text;

			for(byte i = 0; i < author_count; i++)
			{
				book.AddAuthor(authors[i]);
			}

			for(byte i = 0; i < translator_count; i++)
			{
				book.AddTranslator(translators[i]);
			}

			book.Press = TBPublish.Text;
			book.PressSn = TBPubSn.Text;
			book.WordCount = TBWords.Text;

			StorageRecord(rco);
		}

		private (string[], byte) GetPersons(StackPanel panel)
		{
			UIElement uitmp;
			UIElementCollection uis = panel.Children;
			string[] authors = new string[10];
			byte author_count = 0;
			foreach(UIElement ui in uis)
			{
				if(ui is StackPanel)
				{
					uitmp = ((StackPanel)ui).Children[0];
					if(uitmp is TextBox)
					{
						authors[author_count] = ((TextBox)uitmp).Text;
						if(authors[author_count].Length > 0)
							author_count++;
					}
				}
			}

			return (authors, author_count);
		}

		private (string[], byte) GetAuthors()
		{
			return GetPersons(SPAuthors);
		}

		private (string[], byte) GetTranslators()
		{
			return GetPersons(SPTranslators);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if(record != null)
			{
				TBMainTitle.Text = record.Book.MainTitle;
				TBSubTitle.Text = record.Book.SubTitle;
				TBPublish.Text = record.Book.Press;
				TBPubSn.Text = record.Book.PressSn;
				TBWords.Text = record.Book.WordCount;
				TBBeginDate.Text = record.BeginDate;
				TBEndDate.Text = record.EndDate;
				if(record.Status == Record.STATUS_READ)
				{
					RBRead.IsChecked = true;
					byte? star = record.Star;
					if(star is null)
						CBStar.SelectedIndex = -1;
					else
						CBStar.SelectedIndex = star.Value - 1;
					TBComment.Text = record.Comment;
				}
				else
				{
					RBReading.IsChecked = true;
					CBStar.SelectedIndex = 4;
					TBComment.Text = "";
				}

				//author apply
				(string[] authors, byte author_count) = record.Book.GetAuthors();
				if(author_count > 0)
				{
					//The first element
					((TextBox)((StackPanel)(SPAuthors.Children[0])).Children[0]).Text = authors[0];
					if(author_count > 1)
					{
						StackPanel panel;
						for(byte i = 1; i < author_count; i++)
						{
							panel = GenRemovableTextBox(SPAuthors);
							((TextBox)(panel.Children[0])).Text = authors[i];
							SPAuthors.Children.Add(panel);
						}
					}
				}

				//translator apply
				(string[] translators, byte translator_count) = record.Book.GetTranslators();
				if(translator_count > 0)
				{
					((TextBox)((StackPanel)(SPTranslators.Children[0])).Children[0]).Text = translators[0];
					if(translator_count > 1)
					{
						StackPanel panel;
						for(byte i = 1; i < translator_count; i++)
						{
							panel = GenRemovableTextBox(SPTranslators);
							((TextBox)(panel.Children[0])).Text = translators[i];
							SPTranslators.Children.Add(panel);
						}
					}
				}
			}
			else
			{
				//默认值加载。
				CBStar.SelectedIndex = 4; //默认满分评分。
			}
		}

		private void StorageRecord(Record rco)
		{
			XmlNode node = XMLManager.GenRecordNode(rco);
			if(node is null)
			{
				return;
			}

			if(this.record is null)
			{
				//New record
				if(XMLManager.InsertRecord(node))
				{
					MessageBox.Show("保存成功");
					rcoStoraged = rco;
					Close();
				}
				else
				{
					MessageBox.Show("保存失败");
				}
			}
			else
			{
				//Modify existing record
				if(XMLManager.ModifyRecord(node, rco.ID.ToString()))
				{
					MessageBox.Show("保存成功");
					rcoStoraged = rco;
					Close();
				}
				else
				{
					MessageBox.Show("保存失败");
				}
			}
		}
	}
}
