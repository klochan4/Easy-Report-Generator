using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

using System.Text.RegularExpressions;

using ComponentFactory.Krypton.Toolkit;

#warning po kliknuti na cell sa otvori nove okno, kde bude moze vlozit multiline text a taktiez obrazok, po potvrdeni textu sa tento text vlozi do cellu, kde sa zobrazi multiline a cell height sa upravi

namespace Easy_Report_Generator
{
    public partial class Form1 : KryptonForm
    {
        private string document = String.Empty;
        private List<ReplacementDataStructure> dataStructures = new List<ReplacementDataStructure>();
        private BindingSource source;


        public Form1()
        {
            InitializeComponent();


            
            var bindingList = new BindingList<ReplacementDataStructure>(dataStructures);
            source = new BindingSource(bindingList, null);
            kryptonDataGridView1.DataSource = source;



            kryptonDataGridView1.Columns["PropertiesWithTags"].Visible = false;

            kryptonDataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            kryptonDataGridView1.Columns[1].DefaultCellStyle.WrapMode = DataGridViewTriState.True;


            kryptonDataGridView1.Columns[0].ReadOnly = true;
            kryptonDataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            kryptonDataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;


        }

        public static void InsertAPicture(string document, string fileName)
        {
            using (WordprocessingDocument wordprocessingDocument =
                WordprocessingDocument.Open(document, true))
            {
                MainDocumentPart mainPart = wordprocessingDocument.MainDocumentPart;

                ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    imagePart.FeedData(stream);
                }

                AddImageToBody(wordprocessingDocument, mainPart.GetIdOfPart(imagePart));
            }
        }

        private static void AddImageToBody(WordprocessingDocument wordDoc, string relationshipId)
        {
            // Define the reference of the image.
            var element =
                 new Drawing(
                     new DW.Inline(
                         //http://www.ericwhite.com/blog/inserting-an-image-into-a-bookmark-in-an-openxml-wordprocessingml-document/
                         //dimensions of image in EMU
                         new DW.Extent() { Cx = 990000L, Cy = 792000L }, 
                         new DW.EffectExtent()
                         {
                             LeftEdge = 0L,
                             TopEdge = 0L,
                             RightEdge = 0L,
                             BottomEdge = 0L
                         },
                         new DW.DocProperties()
                         {
                             Id = (UInt32Value)1U,
                             Name = "Picture 1"
                         },
                         new DW.NonVisualGraphicFrameDrawingProperties(
                             new A.GraphicFrameLocks() { NoChangeAspect = true }),
                         new A.Graphic(
                             new A.GraphicData(
                                 new PIC.Picture(
                                     new PIC.NonVisualPictureProperties(
                                         new PIC.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = "New Bitmap Image.jpg"
                                         },
                                         new PIC.NonVisualPictureDrawingProperties()),
                                     new PIC.BlipFill(
                                         new A.Blip(
                                             new A.BlipExtensionList(
                                                 new A.BlipExtension()
                                                 {
                                                     Uri =
                                                        "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                 })
                                         )
                                         {
                                             Embed = relationshipId,
                                             CompressionState =
                                             A.BlipCompressionValues.Print
                                         },
                                         new A.Stretch(
                                             new A.FillRectangle())),
                                     new PIC.ShapeProperties(
                                         new A.Transform2D(
                                             new A.Offset() { X = 0L, Y = 0L },
                                             //http://www.ericwhite.com/blog/inserting-an-image-into-a-bookmark-in-an-openxml-wordprocessingml-document/
                                             //dimensions of image in EMU
                                            new A.Extents() { Cx = 990000L, Cy = 792000L }),
                                         new A.PresetGeometry(
                                             new A.AdjustValueList()
                                         )
                                         { Preset = A.ShapeTypeValues.Rectangle }))
                             )
                             { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                     )
                     {
                         DistanceFromTop = (UInt32Value)0U,
                         DistanceFromBottom = (UInt32Value)0U,
                         DistanceFromLeft = (UInt32Value)0U,
                         DistanceFromRight = (UInt32Value)0U,
                         EditId = "50D07946"
                     });

            // Append the reference to body, the element should be in a Run.
            wordDoc.MainDocumentPart.Document.Body.AppendChild(new Paragraph(new Run(element)));
        }


        private string RemoveBetween(string s, char begin, char end)
        {
            Regex regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
            return regex.Replace(s, string.Empty);
        }

        MatchCollection matches;

        // To search and replace content in a document part.
        public void SearchAndReplace(string document)
        {
            if (!File.Exists(document)) {
                MessageBox.Show("Zvolený dokument neexistuje, skontrolujte cestu k súboru");
                return;
            }


            try
            {

                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(document, true))
                {
                    string docText = null;
                    using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                    {
                        docText = sr.ReadToEnd();
                    }

                    //docText = regexText.Replace(docText, "Hi Everyone!");

                    //string t = " of {{RegEx are supported. Validate your expression with Tests mode.}}";

                    string value = @"([{}])\w+";
                    string value1 = @"__(.*?)__"; //with brackets
                    string value2 = @"(?<=\[).+?(?=\])"; //without brackets

                    matches = Regex.Matches(docText, value1);

                    Console.WriteLine("matches count: " + matches.Count);
                    for (int i = 0; i < matches.Count; i++)
                    {
                        string cleanMatch = RemoveBetween(matches[i].ToString(), '<', '>');

                        bool found = false;

                        foreach (ReplacementDataStructure repl in dataStructures)
                        {
                            Console.WriteLine("here");
                            if (repl.CommonPropertyToReplace.Equals(cleanMatch))
                            {
                                Console.WriteLine("cleanMatch: " + cleanMatch);
                                repl.PropertiesWithTags.Add(matches[i].ToString());
                                found = true;
                                break;
                            }
                        }

                        if (!found) {
                            ReplacementDataStructure newR = new ReplacementDataStructure();
                            newR.PropertiesWithTags.Add(matches[i].ToString());
                            newR.CommonPropertyToReplace = cleanMatch;

                            Console.WriteLine("newR.CommonPropertyToReplace: " + newR.CommonPropertyToReplace);
                            Console.WriteLine("matches[i].ToString(): " + matches[i].ToString());

                            dataStructures.Add(newR);
                            source.ResetBindings(true);

                            
                        }

                    }

                }
            }
            catch (System.IO.IOException) {
                MessageBox.Show("Dokument je používaný iným procesom");
            }

        }

        


        private void button1_Click(object sender, EventArgs e)
        {
            //InsertAPicture(document, @"D:\Google Drive Sync\jaspravim\programovanie\Easy Report Generator\docs\IMG_20201001_115056.jpg");
            SearchAndReplace(document);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        void parseTextForOpenXML(Run run, string textualData)
        {
            string[] newLineArray = { Environment.NewLine };
            string[] textArray = textualData.Split(newLineArray, StringSplitOptions.None);

            bool first = true;

            foreach (string line in textArray)
            {
                if (!first)
                {
                    run.Append(new Break());
                }

                first = false;

                Text txt = new Text();
                txt.Text = line;
                run.Append(txt);
            }

        }


            private void button2_Click(object sender, EventArgs e)
        {

            if (!File.Exists(document))
            {
                MessageBox.Show("Zvolený dokument neexistuje, skontrolujte cestu k súboru");
                return;
            }

            WordprocessingDocumentType type;
            string docText = null;


            try { 

                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(document, true))
                {
                    type = wordDoc.DocumentType;
                
                    using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                    {
                        docText = sr.ReadToEnd();
                    }

                    foreach (ReplacementDataStructure dataStr in dataStructures) {
                        foreach (string withTag in dataStr.PropertiesWithTags) {
                            Regex textToFind = new Regex(withTag);



                            matches = Regex.Matches(docText, withTag);

                            Console.WriteLine("matches count: " + matches.Count);
                            for (int i = 0; i < matches.Count; i++)
                            {
                                Console.WriteLine(matches[i].ToString());
                            }

                            if (dataStr.ReplaceWith == null) dataStr.ReplaceWith = String.Empty;



                            docText = textToFind.Replace(docText, dataStr.ReplaceWith.Replace(Environment.NewLine, "<w:br/>"));

                            
                        }
                    }


                    /*
                    foreach (DataGridViewRow row in dataGridView1.Rows) {
                    
                        Regex textToFind = new Regex(row.Cells[0].Value.ToString());

                        //docText = docText.Replace(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString());
                    
                    
                        matches = Regex.Matches(docText, row.Cells[0].Value.ToString());

                        Console.WriteLine("matches count: " + matches.Count);
                        for (int i = 0; i < matches.Count; i++)
                        {
                            Console.WriteLine(matches[i].ToString());
                        }

                        if (row.Cells[2].Value == null) row.Cells[2].Value = "";

                    
                    
                        docText = textToFind.Replace(docText, row.Cells[2].Value.ToString());
                    
                    
                    }

                    */

                    Console.WriteLine("docText: " + docText);
                }

            }


            catch (System.IO.IOException)
            {
                MessageBox.Show("Dokument je používaný iným procesom");
            }

            string outputFile = tb_outputFolder.Text + "\\" + Path.GetFileName(document);
            string oldOutputFile = outputFile;


            int j = 1;
            while (File.Exists(outputFile)) {
                outputFile = oldOutputFile;
                string[] splittedFilename = outputFile.Split('.');

                splittedFilename[0] += " (" + j++ + ")";

                outputFile = splittedFilename[0] + '.' + splittedFilename[1];

                Console.WriteLine("outputFile: " + outputFile);
            }

            
            File.Copy(tb_pathToTemplate.Text, outputFile);
            
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(outputFile, true))
            {
                using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
                {
                    sw.Write(docText);
                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Filter = "Všetky súbory (*.*)|*.*|Dokument Word (*.docx)|*.docx";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        //Get the path of specified file
                        tb_pathToTemplate.Text = openFileDialog.FileName;

                        

                        document = openFileDialog.FileName;

                    }
                }
            }
            catch (Exception ex)
            {
                /*
                sharedMethods.generateErrorLog(this);
                sharedMethods.informUserAndSendException(ex.ToString());
                */
            }
        }


        /// <summary>
        /// Checks if directory is writable by this program
        /// </summary>
        /// <param name="dirPath">Path to directory</param>
        /// <param name="throwIfFails">If true, methods rethrow exceptions if any</param>
        /// <returns></returns>
        public bool IsDirectoryWritable(string dirPath)
        {

            try
            {
                using (FileStream fs = File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch (Exception ex)
            {
                /*
                sharedMethods.generateErrorLog(this);
                sharedMethods.sendErrorMessageToDeveloper("dirPath: " + dirPath);
                sharedMethods.informUserAndSendException(ex.ToString());*/
                return false;
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        string selectPath = fbd.SelectedPath;

                        if (IsDirectoryWritable(selectPath))
                        {
                            tb_outputFolder.Text = fbd.SelectedPath;
                        }
                        else
                        {
                            MessageBox.Show("Program nemá prístup k zvolenému priečinku");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                /*sharedMethods.generateErrorLog(this);
                sharedMethods.informUserAndSendException(ex.ToString());*/

            }
        }

        private void tb_outputFolder_TextChanged(object sender, EventArgs e)
        {

        }

        private void kryptonButton1_Click(object sender, EventArgs e)
        {

        }

        private void kryptonButton2_Click(object sender, EventArgs e)
        {

        }

        private void kryptonButton3_Click(object sender, EventArgs e)
        {

        }

        private void kryptonButton4_Click(object sender, EventArgs e)
        {

        }

        private void tb_pathToTemplate_TextChanged(object sender, EventArgs e)
        {

        }

        private void kryptonTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        private Form_replace form_replace = null;

        private void kryptonButton5_Click(object sender, EventArgs e)
        {
            document = @"D:\Google Drive Sync\jaspravim\programovanie\Easy Report Generator\docs\test_vseobecna_ziadost.docx";
            tb_pathToTemplate.Text = document;
            tb_outputFolder.Text = @"C:\Users\kloky\OneDrive\Desktop";
            SearchAndReplace(document);
        }

        private void kryptonDataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                DataGridViewRow row = kryptonDataGridView1.Rows[e.RowIndex];
                row.Cells[e.ColumnIndex].Selected = false;


                try
                {
                    if (form_replace == null || form_replace.IsDisposed)
                    {
                        string initString = String.Empty;
                        if (row.Cells[e.ColumnIndex].Value != null) initString = row.Cells[e.ColumnIndex].Value.ToString();
                        form_replace = new Form_replace(initString);
                    }

                    form_replace.WindowState = FormWindowState.Normal;
                    


                    var result = form_replace.ShowDialog();
                    form_replace.BringToFront();

                    if (result == DialogResult.OK)
                    {
                        row.Cells[e.ColumnIndex].Value = form_replace.TextOfTextbox;

                        foreach (char character in row.Cells[e.ColumnIndex].Value.ToString().ToCharArray())
                        {
                            Console.WriteLine("char: {0}, value: {1}", character, (int)character);
                        }

                    }

                    form_replace.Dispose();

                }
                catch (Exception ex)
                {
                    /*
                    sharedMethods.generateErrorLog(this);
                    sharedMethods.informUserAndSendException(ex.ToString());
                    */
                }
            }
            catch (Exception ex)
            {

            }    
        }
    }



    public class CustomDataGridViewTextBoxEditingControl : DataGridViewTextBoxEditingControl
    {
        public override bool EditingControlWantsInputKey(
        Keys keyData,
        bool dataGridViewWantsInputKey)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Enter:
                    // Don't let the DataGridView handle the Enter key.
                    return true;
                default:
                    break;
            }

            return base.EditingControlWantsInputKey(keyData, dataGridViewWantsInputKey);
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode & Keys.KeyCode)
            {
                case Keys.Enter:
                    int oldSelectionStart = this.SelectionStart;
                    string currentText = this.Text;

                    this.Text = String.Format("{0}{1}{2}",
                        currentText.Substring(0, this.SelectionStart),
                        Environment.NewLine,
                        currentText.Substring(this.SelectionStart + this.SelectionLength));

                    this.SelectionStart = oldSelectionStart + Environment.NewLine.Length;
                    break;
                default:
                    break;
            }

            base.OnKeyDown(e);
        }
    }

    

    internal partial class ReplacementDataStructure
    {

        [DisplayName("Položka")]
        public string CommonPropertyToReplace { get; set; }

        [DisplayName("Nahradiť za")]
        public string ReplaceWith { get; set; }
        public HashSet<string> PropertiesWithTags { get; set; }


        public ReplacementDataStructure() {
            PropertiesWithTags = new HashSet<string>();
        }
    }
}
