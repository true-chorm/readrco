using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using readrco.src.model;
using readrco.src.tool;

namespace readrco.src.xml
{
	internal static class XMLManager
	{
		private const string TAG = "XMLManager";

		private const string RECORD_FILE_NAME = "records.xml";
		private const string ROOT_NODE_NAME = "readrco";

		private static XmlDocument xml;
		private static List<Record> records;

		/// <summary>
		/// 保证记录存储文件的存在且具有正确的根节点。
		/// 2021-01-29 22:57
		/// </summary>
		internal static bool Init()
		{
			bool isFileEmpty;
			//1. make sure the file exist
			if(File.Exists(RECORD_FILE_NAME))
			{
				isFileEmpty = (new FileInfo(RECORD_FILE_NAME).Length == 0);
			}
			else
			{
				try
				{
					File.Create(RECORD_FILE_NAME).Close();
					isFileEmpty = true;
				}
				catch(Exception e)
				{
					Logger.v(TAG, e.StackTrace);
					return false;
				}
			}

			//2. make sure root node valid
			xml = new XmlDocument();
			if(isFileEmpty)
			{
				if(!CreateRootNode())
				{
					Logger.v(TAG, "Can't create the root node, init failed");
					return false;
				}
			}
			else
			{
				try
				{
					xml.Load(RECORD_FILE_NAME);
					Logger.v(TAG, "xml load success");
				}
				catch(Exception)
				{
					//根节点数据错误。
					try
					{
						File.Move(RECORD_FILE_NAME, RECORD_FILE_NAME + "_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
					}
					catch(Exception e1)
					{
						Logger.v(TAG, "Can't rename the invalid record file.\n" + e1.StackTrace);
						return false;
					}

					try
					{
						File.Create(RECORD_FILE_NAME).Close();
						if(!CreateRootNode())
						{
							Logger.v(TAG, "Can't create the root node, init failed2");
							return false;
						}

						// reload the xml file, 2021-01-31 10:22
						xml.Load(RECORD_FILE_NAME);
						Logger.v(TAG, "reload xml file success");
					}
					catch(Exception e1)
					{
						Logger.v(TAG, "Unpredictable error occur\n" + e1.StackTrace);
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// load record from xml file to memory
		/// 2021-01-31 17:27
		/// </summary>
		internal static bool LoadRecord()
		{
			if(xml is null)
			{
				Logger.v(TAG, "The 'xml' is null");
				return false;
			}

			XmlElement root = xml.DocumentElement;
			if(root is null)
			{
				Logger.v(TAG, "No root node found");
				return false;
			}

			records = new List<Record>(50);
			XmlNode node = root.FirstChild;
			XmlNode node2;
			XmlNodeList nodes;
			XmlNodeList nodes2;
			Record rco;
			Book book;
			while(node != null)
			{
				if(!node.Name.Equals(Record.NODE_RECORD_NAME))
					continue;

				nodes = node.ChildNodes;
				Logger.v(TAG, "Record child count:" + nodes.Count);
				if(nodes.Count != 3) //2021-01-31 17:58
				{
					continue;
				}

				rco = new Record();
				for(byte i = 0; i < 3; i++)
				{
					node2 = nodes[i];
					if(node2 is null)
					{
						rco = null;
						break;
					}

					Logger.v(TAG, "name:" + node2.Name + ", inner text:" + node2.InnerText);
					if(node2.Name.Equals(Record.NODE_ID_NAME))
					{
						try
						{
							int id = int.Parse(node2.InnerText);
							Logger.v(TAG, "ID:" + id);
							rco.ID = id;
						}
						catch(Exception)
						{
							rco = null;
							break;
						}
					}
					else if(node2.Name.Equals(Record.NODE_BOOK_NAME))
					{
						nodes2 = node2.ChildNodes;
						Logger.v(TAG, "'book' child count:" + nodes2.Count);

						book = new Book();
						for(byte j = 0; j < nodes2.Count; j++)
						{
							switch(nodes2[j].Name)
							{
								case Record.NODE_MTITLE_NAME:
									book.MainTitle = nodes2[j].InnerText;
									break;
								case Record.NODE_STITLE_NAME:
									book.SubTitle = nodes2[j].InnerText;
									break;
								case Record.NODE_AUTHORS_NAME:
									XmlNodeList nodes3 = nodes2[j].ChildNodes;
									for(byte k = 0; k < nodes3.Count; k++)
									{
										book.AddAuthor(nodes3[k].InnerText);
									}
									break;
								case Record.NODE_TRANSLATORS_NAME:
									XmlNodeList nodes4 = nodes2[j].ChildNodes;
									for(byte k = 0; k < nodes4.Count; k++)
									{
										book.AddTranslator(nodes4[k].InnerText);
									}
									break;
								case Record.NODE_PRESS_NAME:
									book.Press = nodes2[j].InnerText;
									break;
								case Record.NODE_PRESSSN_NAME:
									book.PressSn = nodes2[j].InnerText;
									break;
								case Record.NODE_WORDCOUNT_NAME:
									book.WordCount = nodes2[j].InnerText;
									break;
							} //switch -- end

							if(book is null)
								break;
						} //for -- end

						if(book is null)
						{
							rco = null;
							break;
						}
						else
						{
							rco.Book = book;
						}
					}
					else if(node2.Name.Equals(Record.NODE_RINFO_NAME))
					{
						nodes2 = node2.ChildNodes;
						Logger.v(TAG, "'read-info' child count:" + nodes2.Count);
						for(byte j = 0; j < nodes2.Count; j++)
						{
							switch(nodes2[j].Name)
							{
								case Record.NODE_STATUS_NAME:
									if(nodes2[j].InnerText.Equals("0"))
									{
										rco.Status = Record.STATUS_READING;
									}
									else
									{
										rco.Status = Record.STATUS_READ;
									}
									break;
								case Record.NODE_BEGINDATE_NAME:
									rco.BeginDate = nodes2[j].InnerText;
									break;
								case Record.NODE_ENDDATE_NAME:
									rco.EndDate = nodes2[j].InnerText;
									break;
								case Record.NODE_STAR_NAME:
									try
									{
										rco.Star = byte.Parse(nodes2[j].InnerText);
									}
									catch(Exception)
									{
										rco = null;
									}
									break;
								case Record.NODE_COMMENT_NAME:
									rco.Comment = nodes2[j].InnerText;
									break;
							} //switch -- end.

							if(rco is null)
							{
								break;
							}
						} //for -- end
					}
					else
					{
						Logger.v(TAG, "Invalid node in 'record' found");
						rco = null;
						break;
					}
				} //for -- end

				if(!IsRecordExisted(rco))
				{
					records.Add(rco);
				}

				node = node.NextSibling;
			} //while -- end

			return true;
		}

		internal static List<Record> GetRecords()
		{
			return records;
		}

		internal static void InsertRecord(int idx, Record rco)
		{
			if(!IsRecordExisted(rco))
			{
				records.Insert(idx, rco);
			}
		}

		private static bool IsRecordExisted(Record rco)
		{
			if(rco is null || records is null)
				return true;

			foreach(Record rco2 in records)
			{
				if(rco2.ID == rco.ID)
					return true;
			}

			return false;
		}

		/// <summary>
		/// 将记录转换为xml节点对象。
		/// </summary>
		internal static XmlNode GenRecordNode(Record rco)
		{
			if(rco is null || xml is null)
			{
				return null;
			}

			XmlElement node = xml.CreateElement(Record.NODE_RECORD_NAME);
			XmlText xmltxt;

			//part 1
			XmlElement id = xml.CreateElement(Record.NODE_ID_NAME);
			xmltxt = xml.CreateTextNode(rco.ID.ToString());
			id.AppendChild(xmltxt);

			//part 2
			XmlElement book = xml.CreateElement(Record.NODE_BOOK_NAME);
			XmlElement mainTitle = xml.CreateElement(Record.NODE_MTITLE_NAME);
			xmltxt = xml.CreateTextNode(rco.Book.MainTitle);
			mainTitle.AppendChild(xmltxt);
			book.AppendChild(mainTitle);
			if(rco.Book.SubTitle.Length > 0)
			{
				XmlElement subTitle = xml.CreateElement(Record.NODE_STITLE_NAME);
				xmltxt = xml.CreateTextNode(rco.Book.SubTitle);
				subTitle.AppendChild(xmltxt);
				book.AppendChild(subTitle);
			}
			XmlElement authors = xml.CreateElement(Record.NODE_AUTHORS_NAME);
			(string[] authorstr, byte author_count) = rco.Book.GetAuthors();
			for(byte i = 0; i < author_count; i++)
			{
				XmlElement author = xml.CreateElement(Record.NODE_AUTHOR_NAME);
				xmltxt = xml.CreateTextNode(authorstr[i]);
				author.AppendChild(xmltxt);

				authors.AppendChild(author);
			}
			book.AppendChild(authors);
			(string[] translatorstr, byte translator_count) = rco.Book.GetTranslators();
			if(translator_count > 0)
			{
				XmlElement translators = xml.CreateElement(Record.NODE_TRANSLATORS_NAME);
				for(byte i = 0; i < translator_count; i++)
				{
					XmlElement translator = xml.CreateElement(Record.NODE_TRANSLATOR_NAME);
					xmltxt = xml.CreateTextNode(translatorstr[i]);
					translator.AppendChild(xmltxt);

					translators.AppendChild(translator);
				}
				book.AppendChild(translators);
			}
			if(rco.Book.Press.Length > 0)
			{
				XmlElement press = xml.CreateElement(Record.NODE_PRESS_NAME);
				xmltxt = xml.CreateTextNode(rco.Book.Press);
				press.AppendChild(xmltxt);
				book.AppendChild(press);
			}
			if(rco.Book.PressSn.Length > 0)
			{
				XmlElement pressSn = xml.CreateElement(Record.NODE_PRESSSN_NAME);
				xmltxt = xml.CreateTextNode(rco.Book.PressSn);
				pressSn.AppendChild(xmltxt);
				book.AppendChild(pressSn);
			}
			if(rco.Book.WordCount != null && rco.Book.WordCount.Length > 0)
			{
				XmlElement wc = xml.CreateElement(Record.NODE_WORDCOUNT_NAME);
				xmltxt = xml.CreateTextNode(rco.Book.WordCount.ToString());
				wc.AppendChild(xmltxt);
				book.AppendChild(wc);
			}

			//part 3
			XmlElement readinfo = xml.CreateElement(Record.NODE_RINFO_NAME);
			XmlElement status = xml.CreateElement(Record.NODE_STATUS_NAME);
			xmltxt = xml.CreateTextNode(rco.Status.ToString());
			status.AppendChild(xmltxt);
			readinfo.AppendChild(status);
			XmlElement bdate = xml.CreateElement(Record.NODE_BEGINDATE_NAME);
			xmltxt = xml.CreateTextNode(rco.BeginDate);
			bdate.AppendChild(xmltxt);
			readinfo.AppendChild(bdate);
			if(rco.Status == Record.STATUS_READ)
			{
				XmlElement edate = xml.CreateElement(Record.NODE_ENDDATE_NAME);
				xmltxt = xml.CreateTextNode(rco.EndDate);
				edate.AppendChild(xmltxt);
				XmlElement star = xml.CreateElement(Record.NODE_STAR_NAME);
				xmltxt = xml.CreateTextNode(rco.Star.ToString());
				star.AppendChild(xmltxt);
				XmlElement comment = xml.CreateElement(Record.NODE_COMMENT_NAME);
				xmltxt = xml.CreateTextNode(rco.Comment);
				comment.AppendChild(xmltxt);
				readinfo.AppendChild(edate);
				readinfo.AppendChild(star);
				readinfo.AppendChild(comment);
			}

			node.AppendChild(id);
			node.AppendChild(book);
			node.AppendChild(readinfo);

			return node;
		}

		internal static bool InsertRecord(XmlNode node)
		{
			if(xml is null)
				return false;

			XmlElement root = xml.DocumentElement;
			if(root is null)
				return false;

			if(root.FirstChild is null)
			{
				root.AppendChild(node);
			}
			else
			{
				root.InsertBefore(node, root.FirstChild);
			}
			xml.Save(RECORD_FILE_NAME);
			Logger.v(TAG, "storage success");
			return true;
		}

		internal static bool ModifyRecord(XmlNode newNode, string idStr)
		{
			if(idStr is null || idStr.Length == 0 || xml is null)
			{
				return false;
			}

			XmlElement root = xml.DocumentElement;
			if(root is null)
			{
				return false;
			}

			XmlNode node = null;
			XmlNode preNode = null;
			XmlNode nextNode;
			XmlNode curNode = root.FirstChild;
			XmlNodeList nodes;

			while(curNode != null)
			{
				nextNode = curNode.NextSibling;

				nodes = curNode.ChildNodes;
				if(nodes.Count == 3)
				{
					foreach(XmlNode nodetmp in nodes)
					{
						if(nodetmp.Name.Equals(Record.NODE_ID_NAME))
						{
							if(nodetmp.InnerText.Equals(idStr))
							{
								node = curNode;
								break;
							}
						}
					}
				}

				if(node != null)
				{
					Logger.v(TAG, "found it, modifying");
					if(preNode is null && nextNode is null)
					{
						Logger.v(TAG, "impossible node modifying");
						//Impossible
					}
					if(preNode is null)
					{
						Logger.v(TAG, "head node modifying");
						//The head node
						root.RemoveChild(node);
						root.InsertBefore(newNode, nextNode);
					}
					else if(nextNode is null)
					{
						Logger.v(TAG, "rear node modifying");
						//The rear node
						root.RemoveChild(node);
						root.InsertAfter(newNode, preNode);
					}
					else
					{
						Logger.v(TAG, "inside node modifying");
						//Inside node
						root.RemoveChild(node);
						root.InsertAfter(newNode, preNode);
					}
					xml.Save(RECORD_FILE_NAME);
					return true;
				}
				else
				{
					preNode = curNode;
					curNode = nextNode;
				}
			}

			return false;
		}

		private static bool CreateRootNode()
		{
			XmlElement root = xml.CreateElement(ROOT_NODE_NAME);
			try
			{
				xml.AppendChild(root);
				xml.Save(RECORD_FILE_NAME);
			}
			catch(Exception)
			{
				return false;
			}

			return true;
		}
	}
}
