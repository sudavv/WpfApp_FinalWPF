using System;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using System.Collections.Generic;
using WpfApp_FinalWPF.Properties;
using System.Resources;
using System.Collections;
using System.Drawing;
using System.Linq;

namespace WpfApp_FinalWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        Dictionary<int, string> Buffer = new Dictionary<int, string>();
        bool reset;
        public MainWindow()
        {
            InitializeComponent();
            try
            { 
            IResourceReader reader = new ResourceReader(@"res.ini"); // читаем файл конфига с координатами формы
                IDictionaryEnumerator dict = reader.GetEnumerator();
                foreach (DictionaryEntry par in reader)
                {
                    string key = (par.Key as string);
                    if (key == "Top")
                    {
                        Top = (Convert.ToInt32(par.Value));
                    }
                    else if (key == "Left")
                    {
                        Left = (Convert.ToInt32(par.Value));
                    }
                    else if (key == "Height")
                    {
                        Height = (Convert.ToInt32(par.Value));
                    }
                    else if (key == "Width")
                    {
                        Width = (Convert.ToInt32(par.Value));
                    }
                }
                reader.Dispose();
                reader.Close();
            }
            catch
            { // если нет конфига, то располагаем форму в центре экрана
                //Left = System.Windows.SystemParameters.PrimaryScreenWidth;
                //Top = System.Windows.SystemParameters.PrimaryScreenHeight;
                return;
            }
        }

        private void ExitExec(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string oper = (sender as System.Windows.Controls.Button).Content.ToString(); //определяем название нажатой кнопки          
            {
                AddToDict(oper, false);
            }
            UnfocusElement.Focus(); // снимаем фокус с кнопки, чтобы работал Enter
        }


        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
          AddToDict(e, true, sender);

            UnfocusElement.Focus(); // снимаем фокус с кнопки, чтобы работал Enter
        }

        public void AddToDict(object key, bool keyboard, object sender = null) //заполняем словарь
        {   
            int count = Buffer.Count;
            string strkey = "";
            char operChar = ' ';
            if (keyboard)// ввод с клавиатуры
            {
                object obj;
                obj = true;
                System.Windows.Input.KeyEventArgs e = key as System.Windows.Input.KeyEventArgs;
                if (!sender.Equals(obj) && e.Key == Key.Back) //если сработал backspace из Window_KeyDown, то скипаем
                {
                    return;
                }
                else if (sender.Equals(obj) && !(e.Key == Key.Back)) //если сработала другая клавиша из Window_PreviewKeyDown 
                {
                    return;
                }

               
                if ((e.Key >= Key.D0 && e.Key <= Key.D9) | (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)) //определяем, какую клавишу нажал пользователь
                {                  
                    if (reset)
                    {
                        Buffer.Clear();
                        WorkField.Text = "0";
                        SubWorkField.Text = "";
                        reset = false;
                        count = 0;
                    }
                    WorkField.Text = e.Key.ToString();
                    strkey = e.Key.ToString();
                    operChar = strkey[strkey.Length-1];
                    strkey = operChar.ToString();
                    WorkField.Text += strkey;
                }
                else if (e.Key == Key.Enter | e.Key == Key.OemPlus) // нажали enter - считаем
                {
                    Compute();
                }       
                else if (e.Key == Key.Add)
                {
                    operChar = '+';
                }
                else if (e.Key == Key.Subtract | e.Key == Key.OemMinus)
                {
                    operChar = '-';
                }
                else if (e.Key == Key.Multiply)
                {
                    operChar = '*';
                }
                else if (e.Key == Key.Divide)
                {
                    operChar = '/';
                }
                else if (e.Key == Key.Decimal)
                {
                    operChar = ',';
                    strkey = ",";
                }
                else if (e.Key == Key.Back) //backspace
                {
                    if (count > 0)
                    {
                        if (Buffer[count].Length < 1)
                        {
                            WorkField.Text = "0";
                            Buffer.Remove(count);
                            return;
                        }
                    }
                        operChar = '<';
                }
            }
            else // ввод с формы
            {
                strkey = key as string; 
                
                 if (strkey == "C")// очищаем буфер
                {
                    Buffer.Clear();
                    WorkField.Text = "0";
                    SubWorkField.Text = "";
                    return;
                }

                else if (strkey == "CE")// очищаем последнюю запись и делаем возврат
                {
                    if (count > 0)
                    {                        
                        if (LastDigit())// последняя запись - число, убираем
                        {
                            Buffer.Remove(count);
                        }                        
                    }
                    WorkField.Text = "0";
                    return;                    
                }
                 else if (strkey == "+/-")
                {                    
                    if (count == 0)
                    {
                        return;
                    }
                   
                        Buffer.Add(count+1, "*(-1)");
                        Compute();
                        return;
                }
                
                operChar = Convert.ToChar(strkey);
            }

            if (Char.IsDigit(operChar))//число - заносим в словарь
            {
                if (reset)
                {
                    Buffer.Clear();
                    WorkField.Text = "0";
                    SubWorkField.Text = "0";
                    count = 0;
                    reset = false;
                }
                    WorkField.Text = strkey;

                if (count == 0) //Буфер пуст, добавляем первый элемент
                {
                    Buffer.Add(1, strkey);
                }
                else if (Buffer[count] == "/" | Buffer[count] == "+" | Buffer[count] == "-" | Buffer[count] == "*" | Buffer[count] == "%") //не пуст, но последний знак арифметический - добавляем новую строку
                {
                    Buffer.Add(count + 1, strkey);
                }
                else //последний элемент - число, дописываем в него
                {
                    Buffer[count] = Buffer[count] += strkey;
                    WorkField.Text = Buffer[count];
                }
            }
            else if (operChar == '/' | operChar == '+' | operChar == '-' | operChar == '*' && count>0)// арифметический знак, создаем новую запись в буфере и кладём туда
            {

                if (reset)
                {
                    SubWorkField.Text = WorkField.Text;

                    Buffer.Clear();
                    Buffer.Add(1, WorkField.Text);
                    Buffer.Add(2, operChar.ToString());
                    WorkField.Text = "0";
                }
                else
                {
                    if (!LastDigit())
                    {
                        Buffer.Remove(count);
                        count--;
                    }

                    Buffer.Add(count + 1, operChar.ToString());
                    SubWorkField.Text = Sum(count);
                }

                reset = false;
            }
            else if (operChar == '=' && count > 0)
            {
                Compute();
            }
            else if (operChar == ',' && count > 0)
            {
                if (LastDigit())
                {
                    if (!Buffer[count].Contains(","))
                        {
                        Buffer[count] = Buffer[count] += strkey;
                        WorkField.Text = Buffer[count];
                        }
                }
                reset = false;

            }
            else if (operChar == '<' && count > 0)
            {
                if (LastDigit())
                {
                    Buffer[count] = Buffer[count].Substring(0, Buffer[count].Length - 1);
                    WorkField.Text = WorkField.Text.Substring(0, WorkField.Text.Length - 1);
                }
            }
            else if (operChar == '%' && count > 0) //считаем процент
            {
                if (count == 3)
                {
                    double a = 0;
                    string symbol = "";
                    double c = 0;
                    try
                    {
                        a = Convert.ToDouble(Buffer[1]);
                        symbol = Buffer[2];
                        c = Convert.ToDouble(Buffer[3]);
                        if (a < 1 | c <1)
                        {
                            reset = false;
                            return;
                        }
                    }
                    catch
                    {
                        reset = false;
                        return;
                    }
                    if (symbol == "*")
                    {
                        a = ((a / 100)*c)*a;                      
                    }
                    else if (symbol == "/")
                    {
                        a = 10/((a / 100) * c);
                    }
                    else if (symbol == "+")
                    {
                        a = 10 + ((a / 100) * c);
                    }
                    else if (symbol == "-")
                    {
                        a = 10 - ((a / 100) * c);
                    }
                    
                    WorkField.Text = a.ToString();
                    SubWorkField.Text = Buffer[1] + " " + Buffer[2] + " " + Buffer[3] + " %";
                    Buffer.Clear();
                }
                else
                {
                    Buffer.Clear();
                    WorkField.Text = "0";
                    SubWorkField.Text = "";
                }

            }
        }

        public void Compute() //вычисляем выражение
        {
            int count = Buffer.Count;
            string change = "";

            while (true)
            {
                if (!LastDigit()) // в конце строки есть символ - убираем
                {
                    if (Buffer[count].Length > 1)//смена знака
                    {
                        change = "-";
                    }
                    Buffer.Remove(count);
                    count--;
                    if (SubWorkField.Text.Length > 0)
                    { 
                    SubWorkField.Text = SubWorkField.Text.Substring(0, SubWorkField.Text.Length - 1);
                    }                  
                }
                else
                    break;
            }                         
           
            string sum = Sum(count);           
            
            DataTable dt = new DataTable();
            string str = dt.Compute(sum.Replace(',', '.'), "").ToString();
            if (str[0] == '-' && change == "-")
            {
                change = "";
                str = str.Substring(2);
                WorkField.Text = change + str;
                SubWorkField.Text = "neg(" + sum + ")";
                reset = true;
                return;
            }
            WorkField.Text = change + str;
            SubWorkField.Text = sum;
            reset = true;            
        }

        public string Sum(int count)
        {
            string sum = "";
            for (int i = 1; i <= count; i++) // составляем строку из всех элементов, чтобы посчитать
            {
                sum = sum += Buffer[i];
            }
            return sum;
        }

        public bool LastDigit() //проверяем, число ли последняя запись в буфере
        {
           int count = Buffer.Count;
            string laststr = Buffer[count];
            char lastchar = laststr[laststr.Length - 1];
            if (Char.IsDigit(lastchar))// последняя запись - число
            {
                return true;
            }
            return false;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IResourceWriter writer = new ResourceWriter(@"res.ini");
            writer.AddResource("Left", Left);
            writer.AddResource("Top", Top);
            writer.AddResource("Height", Height);
            writer.AddResource("Width", Width);
            writer.Dispose();
            writer.Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(WorkField.Text);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            string clipb = System.Windows.Clipboard.GetText();
            
            if (CheckChar(clipb))
            {
                WorkField.Text = clipb;
            }            
        }

        public bool CheckChar(string text)
        {
            text = text.Replace('.', ',');
            for (int i = 0; i < text.Length; i++)
            {
                if (!(Char.IsDigit(text[i]) | text[i] == ','))
                {
                    return false;
                }                
            }

            if (text.Count(f => f == ',')>1)
            {
                return false;
            }

            return true;
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Window_KeyDown(true, e);
        }
    }   
}
