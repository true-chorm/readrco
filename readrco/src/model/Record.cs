using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace readrco.src.model
{
	public class Record : IComparable, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private readonly PropertyChangedEventArgs pcID;
		private readonly PropertyChangedEventArgs pcBeginDate;
		private readonly PropertyChangedEventArgs pcEndDate;
		private readonly PropertyChangedEventArgs pcStatus;
		private readonly PropertyChangedEventArgs pcStar;

		internal const byte STATUS_READING = 0;
		internal const byte STATUS_READ = 1;

		internal const string NODE_RECORD_NAME = "record";
		internal const string NODE_ID_NAME = "id";
		internal const string NODE_BOOK_NAME = "book";
		internal const string NODE_RINFO_NAME = "read_info";
		internal const string NODE_MTITLE_NAME = "main_title";
		internal const string NODE_STITLE_NAME = "sub_title";
		internal const string NODE_AUTHORS_NAME = "authors";
		internal const string NODE_AUTHOR_NAME = "author";
		internal const string NODE_TRANSLATORS_NAME = "translators";
		internal const string NODE_TRANSLATOR_NAME = "translator";
		internal const string NODE_PRESS_NAME = "press";
		internal const string NODE_PRESSSN_NAME = "press_sn";
		internal const string NODE_WORDCOUNT_NAME = "word_count";
		internal const string NODE_STATUS_NAME = "status";
		internal const string NODE_BEGINDATE_NAME = "begin_date";
		internal const string NODE_ENDDATE_NAME = "end_date";
		internal const string NODE_STAR_NAME = "star";
		internal const string NODE_COMMENT_NAME = "comment";

		private int id;
		private string beginDate;
		private string endDate;
		private byte status;
		private byte? star;

		internal Record()
		{
			pcID = new PropertyChangedEventArgs("ID");
			pcBeginDate = new PropertyChangedEventArgs("BeginDate");
			pcEndDate = new PropertyChangedEventArgs("EndDate");
			pcStatus = new PropertyChangedEventArgs("Status");
			pcStar = new PropertyChangedEventArgs("Star");
		}

		public int ID
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
				if(PropertyChanged != null)
				{
					PropertyChanged.Invoke(this, pcID);
				}
			}
		}

		public Book Book
		{
			get;
			set;
		}

	    public byte Status
		{
			get
			{
				return status;
			}
			set
			{
				status = value;
				if(PropertyChanged != null)
				{
					PropertyChanged.Invoke(this, pcStatus);
				}
			}
		}

		public string BeginDate
		{
			get
			{
				return beginDate;
			}
			set
			{
				beginDate = value;
				if(PropertyChanged != null)
				{
					PropertyChanged.Invoke(this, pcBeginDate);
				}
			}
		}

		public string EndDate
		{
			get
			{
				return endDate;
			}
			set
			{
				endDate = value;
				if(PropertyChanged != null)
				{
					PropertyChanged.Invoke(this, pcEndDate);
				}
			}
		}

		public byte? Star
		{
			get
			{
				if(star is null)
					return null;
				else
					return star.Value;
			}
			set
			{
				star = value;
				if(PropertyChanged != null)
				{
					PropertyChanged.Invoke(this, pcStar);
				}
			}
		}

		internal string Comment
		{
			get;
			set;
		}

		internal void BookChangedCallback(string name, object value)
		{

		}

		public override string ToString()
		{
			if(this.Book is null)
				return "";

			return "\nID:" + ID + "\nMainTitle:" + this.Book.MainTitle + "\nSubTitle:" + this.Book.SubTitle + "\nStar:" + Star + "\nComment:" + Comment;
		}

		public int CompareTo(object obj)
		{
			if(obj is Record rco)
			{
				if(ID > rco.ID)
					return 1;
				else if(ID < rco.ID)
					return -1;
				else
					return 0;
			}

			return 0; //Don't compare
		}
	}
}
