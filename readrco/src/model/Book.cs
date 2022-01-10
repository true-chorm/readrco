using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using readrco.src.tool;

namespace readrco.src.model
{
	public class Book : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private readonly PropertyChangedEventArgs pcMainTitle;
		private readonly PropertyChangedEventArgs pcSubTitle;

		private byte author_count;
		private byte translator_count;

		private string mainTitle;
		private string subTitle;
		private string[] authors;
		private string[] translators;

		internal Book()
		{
			authors = new string[1];
			translators = new string[1];
			pcMainTitle = new PropertyChangedEventArgs("MainTitle");
			pcSubTitle = new PropertyChangedEventArgs("SubTitle");
		}

		public string MainTitle
		{
			get
			{
				return mainTitle;
			}
			set
			{
				mainTitle = value;
				if(PropertyChanged != null)
				{
					PropertyChanged.Invoke(this, pcMainTitle);
				}
			}
		}

		public string SubTitle
		{
			get
			{
				return subTitle;
			}
			set
			{
				subTitle = value;
				if(PropertyChanged != null)
				{
					PropertyChanged.Invoke(this, pcSubTitle);
				}
			}
		}

		internal string Press
		{
			get;
			set;
		}

		internal string PressSn
		{
			get;
			set;
		}

		/// <summary>
		/// kilo word count
		/// </summary>
		internal string WordCount
		{
			get;
			set;
		}

		internal (string[], byte) GetAuthors()
		{
			return (authors, author_count);
		}

		/// <summary>
		/// 允许添加重复名称。
		/// </summary>
		internal void AddAuthor(string author)
		{
			string[] tmp = AddString(author, authors, ref author_count);
			if(tmp != null)
			{
				authors = tmp;
			}
		}

		internal void RemoveAuthor(string author)
		{
			string[] tmp = RemoveString(author, authors, ref author_count);
			if(tmp != null)
			{
				authors = tmp;
			}
		}

		internal void ClearAuthors()
		{
			authors = new string[1];
			author_count = 0;
		}

		internal (string[], byte) GetTranslators()
		{
			return (translators, translator_count);
		}

		internal void AddTranslator(string translator)
		{
			string[] tmp = AddString(translator, translators, ref translator_count);
			if(tmp != null)
				translators = tmp;
		}

		internal void RemoveTranslator(string translator)
		{
			string[] tmp = RemoveString(translator, translators, ref translator_count);
			if(tmp != null)
			{
				translators = tmp;
			}
		}

		internal void ClearTranslators()
		{
			translators = new string[1];
			translator_count = 0;
		}

		private string[] AddString(string str, string[] which, ref byte which_count)
		{
			Logger.v("book", "add string,str:" + str + ",count:" + which_count);
			if(str == null || str.Length == 0 || which == null)
				return null;

			if(which_count < which.Length)
			{
				which[which_count++] = str;
				return which;
			}
			else
			{
				//expand array
				string[] tmp = new string[which_count + 1];
				for(byte i = 0; i < which_count; i++)
				{
					tmp[i] = which[i];
				}

				tmp[which_count++] = str;

				return tmp;
			}
		}

		private string[] RemoveString(string str, string[] which, ref byte which_count)
		{
			if(str == null || str.Length == 0 || which == null || which_count == 0)
				return null;

			string[] tmp = new string[which_count];
			byte j = 0;
			for(byte i = 0; i < which_count; i++)
			{
				if(!which[i].Equals(str))
				{
					tmp[j++] = which[i];
				}
			}

			which_count = j;
			return tmp;
		}
	}
}
