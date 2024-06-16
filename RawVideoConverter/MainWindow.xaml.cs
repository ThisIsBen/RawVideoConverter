using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RawVideoConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Init the UI
            InitUI();
        }
        //Init the UI
        private void InitUI()
        {

            //Load in previous settings from a file
            var previousSettings = FileIO.readAllLinesOfAFile(".previousSettings.txt");
            if (previousSettings != null)
            {
                if (previousSettings[0] == radBtn_Manual.Content.ToString())
                {
                    radBtn_Manual.IsChecked = true;

                }
                else if (previousSettings[0] == radBtn_Auto.Content.ToString())
                {
                    radBtn_Auto.IsChecked = true;

                }

                txtBox_InputDir.Text = previousSettings[1];
                txtBox_OutputDir.Text = previousSettings[2];
            }
            
           



        }
        

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //Extract user input info
            string mode="";
            string radBtn_Manual_Val= radBtn_Manual.Content.ToString();
            string radBtn_Auto_Val = radBtn_Auto.Content.ToString();

            

            if (radBtn_Manual.IsChecked == true)
            {
                mode = radBtn_Manual_Val;

            }
            else
            {
                mode = radBtn_Auto_Val;
            }

            //Save settings to a file
            string settings = $"{mode}\n{txtBox_InputDir.Text}\n{txtBox_OutputDir.Text}";
            FileIO.outputStringsToFile(".previousSettings.txt", settings);


            //Create an object of converter and apply user's input info
            Converter converterObj = new Converter(this , mode, monthPicker.DisplayDate, txtBox_InputDir.Text, txtBox_OutputDir.Text, radBtn_Manual_Val, radBtn_Auto_Val);

            //Start converting
            try
            {
                converterObj.start();
            }
            catch(Exception ex)
            {
                logging($"Error occured: \n{ ex.ToString()}");
            }

           

        }


        





        //Output current status to the UI
        public   void logging(string text)
        {
            var now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");


            this.Dispatcher.Invoke((Action)(() =>
            {
                txtBox_Status.Text += $"{now}\n  {text}\n";
                txtBox_Status.ScrollToEnd();
            }));
           
        }

        private void radBtn_Manual_Checked(object sender, RoutedEventArgs e)
        {
            radBtn_Auto.IsChecked = false;
        }

        private void radBtn_Auto_Checked(object sender, RoutedEventArgs e)
        {
            radBtn_Manual.IsChecked = false;
        }




        private void txtBox_InputDir_Drop(object sender, DragEventArgs e)
        {
            var dropFiles = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (dropFiles == null) return;
            txtBox_InputDir.Text = dropFiles[0];
            //// Get data object
            //var dataObject = e.Data as DataObject;

            //// Check for file list
            //if (dataObject.ContainsFileDropList())
            //{
            //    // Clear values
            //    txtBox_InputDir.Text = string.Empty;



            //    // Set text
            //    txtBox_InputDir.Text = dataObject.GetText();
            //}
        }

       
        private void txtBox_InputDir_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            e.Handled = true;
        }



        

        private void txtBox_OutputDir_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            e.Handled = true;
        }
        private void txtBox_OutputDir_Drop(object sender, DragEventArgs e)
        {

            var dropFiles = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (dropFiles == null) return;
            txtBox_OutputDir.Text = dropFiles[0];
            //// Get data object
            //var dataObject = e.Data as DataObject;

            //// Check for file list
            //if (dataObject.ContainsFileDropList())
            //{
            //    // Clear values
            //    txtBox_OutputDir.Text = string.Empty;



            //    // Set text
            //    txtBox_OutputDir.Text = dataObject.GetText();
            //}
        }

        private void monthPicker_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            Calendar calObj = sender as Calendar;

            calObj.DisplayMode = CalendarMode.Year;
        }

        private void monthPicker_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured is CalendarItem)
            {
                Mouse.Capture(null);
            }
        }
    }
}
