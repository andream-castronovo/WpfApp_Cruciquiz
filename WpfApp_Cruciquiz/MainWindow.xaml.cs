using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32; // Libreria per OpenFileDialog()


namespace WpfApp_Cruciquiz
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Programmato da Andrea Maria Castronovo, 4°I, Data inizio: 24/09/2022
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Campi di classe

        string[,] _matriceLettere; // Contiene le lettere del puzzle
        Button[,] _btns; // Contiene i bottoni grafici
        
        // Righe e colonne consentite in base alla grandezza della finestra
        double _R;
        double _C; 

        // Colori da usare
        SolidColorBrush COLORE = Brushes.LightGreen;
        SolidColorBrush COLORE_NUOVO = Brushes.LightSalmon;

        #endregion



        #region Eventi oggetti UI
        private void btnLoadMatrix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //CaricaMatriceDaFile("../../puzzle2.txt");
                CaricaMatriceDaFile(CercaFile(), ' ',(int) _R,(int) _C);
                
                _btns = new Button[_matriceLettere.GetLength(0), _matriceLettere.GetLength(1)];
                
                GeneraBottoni(_btns);
                ScriviInBottoni(_btns, _matriceLettere);

                grdColorIndex.Visibility = Visibility.Visible;
                grdSearchWord.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                MessaggioErrore(ex);
            }

        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            txtWordToSearch.Focus();
            txtWordToSearch.SelectAll();
            string parolaDaCercare = txtWordToSearch.Text.ToUpper();
            try
            {
                // CercaParolaSxDx(parolaDaCercare, _matriceLettere);
                // CercaParolaDxSx(parolaDaCercare, _matriceLettere);
                // CercaParolaUpDown(parolaDaCercare, _matriceLettere);
                // CercaParolaDxSx(parolaDaCercare, _matriceLettere);
                if (!CercaIniziale(parolaDaCercare, _matriceLettere, _btns))
                    MessageBox.Show("Nessuna parola trovata","Info",MessageBoxButton.OK,MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessaggioErrore(ex);
            }
            
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _R = Math.Ceiling((12 * e.NewSize.Width) / 1200);
            _C = Math.Ceiling((14 * e.NewSize.Height) / 675);

            lblDimConsentite.Content = $"Max puzzle {_R}x{_C}";

            if (_matriceLettere == null)
                return;

            if (_matriceLettere.GetLength(0) > _R || _matriceLettere.GetLength(1) > _C)
            {
                CancellaMatrici(ref _matriceLettere, ref _btns, grdMatrix);
            }
        }

        #endregion

        /// <summary>
        /// Annulla le matrici e le rimuove dalla griglia
        /// </summary>
        /// <param name="mat">Matrice di stringhe da azzerare</param>
        /// <param name="btns">Matrice di bottoni da azzerare</param>
        /// <param name="grd">Griglia da cui togliere i bottoni</param>
        void CancellaMatrici(ref string[,] mat, ref Button[,] btns, Grid grd)
        {
            if (mat == null || btns == null)
                return;

            mat = null;

            for (int i = 0; i < btns.GetLength(0); i++)
            {
                for (int j = 0; j < btns.GetLength(1); j++)
                {
                    grd.Children.Remove(btns[i, j]);
                    btns[i, j] = null;
                }
            }

            btns = null;
        }


        #region Carica la matrice di lettere dal file in una matrice 
        
        /// <summary>
        /// Apre la finestra di ricerca di un file sul computer
        /// </summary>
        /// <returns>Percorso completo file selezionato</returns>
        /// <exception cref="Exception">Nessun file selezionato</exception>
        string CercaFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            
            if ((bool) ofd.ShowDialog())
                return ofd.FileName;

            CancellaMatrici(ref _matriceLettere, ref _btns, grdMatrix);
            
            Width = (13 * 1200) / 12;
            Height = (15 * 675) / 14;


            return @"..\..\Crucipuzzle.txt";
            
        }
        /// <summary>
        /// Converte una matrice di lettere da un file di testo a una matrice di stringhe
        /// </summary>
        /// <param name="file">Percorso del file contenente la matrice</param>
        /// <param name="separatore">Divisore delle lettere</param>
        /// <exception cref="Exception">Matrice troppo grande</exception>
        void CaricaMatriceDaFile(string file, char separatore, int maxRows, int maxCols)
        {
            #region Converto il file in una stringa lunga
            StreamReader sr = new StreamReader(file);
            string strFile = sr.ReadToEnd().Replace("\r",""); // \r è il carattere che fa muovere il cursore
                                                              // all'inizio nel terminale ed essendo inutile va rimosso.
            sr.Close();
            #endregion
            
            
            
            string[] righe = strFile.Split('\n');

            if (strFile.IndexOf("\n") == -1) // Se il file ha una riga sola ci aggiungo lo \n per farlo funzionare
                strFile += "\n";

            int colonne = strFile.Replace(separatore.ToString(), "").IndexOf('\n'); // L'indice di \n è il numero di caratteri, quindi colonne

            if (righe.Length > maxRows || colonne > maxCols)
                throw new Exception($"Matrice di dimensione troppo grande rispetto alla finestra. Dimensioni consentite per grandezza attuale: {_R}x{_C}, ingrandire la finestra per permettere matrici più ampie");
            
            _matriceLettere = new string[righe.Length, colonne];

            for (int i = 0; i < _matriceLettere.GetLength(0); i++)
            {
                string[] lettere = righe[i].Split(separatore); // Splitto nuovamente per avere le lettere singole 
                
                for (int j = 0; j < lettere.Length; j++)
                {
                    _matriceLettere[i, j] = lettere[j];
                }
            }
        }
        #endregion

        #region Matrice Bottoni

        void GeneraBottoni(Button[,] btns)
        {
            // Dimensione griglia in base a quanti bottoni ci sono
            grdMatrix.Width = 50 * btns.GetLength(1); 
            grdMatrix.Height = 50 * btns.GetLength(0);

            for (int i = 0; i < btns.GetLength(0); i++)
            {
                for (int j = 0; j < btns.GetLength(1); j++)
                {
                    Button btn = new Button()
                    {
                        Width = grdMatrix.Width / btns.GetLength(1),
                        Height = grdMatrix.Height / btns.GetLength(0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        FontSize = 25,
                };
                    
                    btn.Margin = new Thickness(btn.Width * j, btn.Height * i, 0, 0);

                    btns[i, j] = btn;

                    grdMatrix.Children.Add(btn);

                }
            }
        }

        void ScriviInBottoni(Button[,] btns, string[,] mat)
        {
            for (int i = 0; i < btns.GetLength(0); i++)
            {
                for (int j = 0; j < btns.GetLength(1); j++)
                {
                    btns[i, j].Content = mat[i, j];
                }
            }
        }
        #endregion


        #region Nuovo metodo cerca parola
        /// <summary>
        /// Cerca l'iniziale della parola da ricercare nel puzzle
        /// </summary>
        /// <param name="parolaDaCercare">Parola da cercare</param>
        /// <param name="lettere">Matrice che contiene tutte le lettere</param>
        /// <param name="btns">Matrice con tutti i bottoni da colorare</param>
        /// <returns>false se nessuna parola è stata trovata; true se anche solo una parola è stata trovata</returns>
        /// <exception cref="Exception">Nessuna parola da cercare</exception>
        bool CercaIniziale(string parolaDaCercare, string[,] lettere, Button[,] btns)
        {
            if (parolaDaCercare == "")
                throw new Exception("Non hai inserito una parola");

            AggiornaColori(btns);

            int paroleTrovate = 0;
            for (int i = 0; i < lettere.GetLength(0); i++) // Due for per lo scorrere della matrice
            {
                for (int j = 0; j < lettere.GetLength(1); j++) 
                {
                    if (parolaDaCercare[0].ToString() == lettere[i, j]) // Rileva l'iniziale della parola scelta
                    {
                        //if (parolaDaCercare.Length != 1)
                        //{
                        for (int dX = -1; dX<=1; dX++) // Due for per girare tutte le direzioni possibili: -1,0,1
                        {
                            for (int dY = -1; dY <= 1; dY++) 
                            {
                                ProvaParola(parolaDaCercare, lettere, btns, dX, dY, i, j, ref paroleTrovate);
                            }
                        }
                        //}
                        //else
                        //{
                        //    btns[i, j].Background = COLORE_NUOVO;
                        //}
                    }
                }
            }

            if (paroleTrovate == 0)
                return false;
            
            return true;

        }



        /// <summary>
        /// Metodo per provare a comporre la parola partendo dall'iniziale della parola data
        /// </summary>
        /// <param name="parolaDaCercare">Parola da cercare</param>
        /// <param name="lettere">Matrice contenente le lettere del puzzle</param>
        /// <param name="orizzontale">Direzione orizzontale. Può essere -1, 0 o 1</param>
        /// <param name="verticale">Direzione orizzontale. Può essere -1, 0 o 1</param>
        /// <param name="i">Riga della matrice</param>
        /// <param name="j">Colonna della matrice</param>
        void ProvaParola(string parolaDaCercare, string[,] lettere, Button[,] btns , int orizzontale, int verticale, int i, int j, ref int paroleTrovate)
        {
            string parola = "";
            // Se la parola è lunga 1 non entra nel for
            for (int k = 0; k < parolaDaCercare.Length; k++)
            {
                if (
                    j + (k * orizzontale)  >= lettere.GetLength(1) // Controllo in caso esco dalla matrice lateralmente
                    ||
                    i + (k * verticale) >= lettere.GetLength(0) // Controllo in caso esco dalla matrice verticalmente
                    ||
                    j + (k * orizzontale) < 0 // Controllo in caso esco dalla matrice lateralmente da sinistra
                    || 
                    i + (k * verticale) < 0 // Controllo in caso esco dalla matrice verticalmente da sinistra
                    )
                    return;


                // [ righe o colonne + (carattere attuale della parola * la direzione) ]
                string check = lettere[i+(k*verticale), j+(k*orizzontale)];
                parola += check; // Compongo la parola lettera per lettera
                
                //Console.WriteLine(parolaDaCercare.Substring(0,k+1) + " VS " + parola);
                
                if (parolaDaCercare.Substring(0, k + 1) != parola) // Se una lettera trovata fino ad ora è diversa esci
                    return;

                if (parola == parolaDaCercare) // Se trovi la parola, ricomincia, colora i bottoni e aggiungi 1 alle parole trovate
                {
                    parola = "";
                    
                    for (int l = 0; l < parolaDaCercare.Length; l++)
                    {
                        btns[i + (l * verticale), j + (l * orizzontale)].Background = COLORE_NUOVO;
                    }
                    paroleTrovate++;
                }
            }
        }
        /// <summary>
        /// Aggiorna i bottoni dal colore nuovo a quello vecchio
        /// </summary>
        /// <param name="btns"></param>
        void AggiornaColori(Button[,] btns)
        {
            for (int i = 0; i < btns.GetLength(0); i++)
            {
                for (int j = 0; j < btns.GetLength(1); j++)
                {
                    if (btns[i,j].Background == COLORE_NUOVO)
                        btns[i,j].Background = COLORE;
                }
            }
        }


        #endregion





        #region Vecchio metodo cerca parola

        // Vecchio metodo per cercare la parola
        // Non funziona diagonalmente


        //#region >> Cerca la parola da sinistra verso destra >>
        ///// <summary>
        ///// Cerca una parola data come parametro in una matrice di lettere
        ///// </summary>
        ///// <param name="parolaDaCercare">Parola da cercare</param>
        ///// <param name="lettere">Matrice di lettere</param>
        //void CercaParolaSxDx(string parolaDaCercare, string[,] lettere)
        //{
        //    string parola = "";
        //    Button[,] bottoniDaColorare = new Button[lettere.GetLength(0),lettere.GetLength(1)];
        //    bool isFound = false;
        //    int trovate = 0;


        //    for (int i = 0; i < _matriceLettere.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < _matriceLettere.GetLength(1); j++)
        //        {

        //            CercaParola(
        //                i,
        //                j,
        //                ref isFound,
        //                ref parola,
        //                ref bottoniDaColorare,
        //                ref trovate,
        //                parolaDaCercare,
        //                _matriceLettere,
        //                COLORE_SXDX
        //                );
        //        }

        //        // Resetta la ricerca della parola se finisce la riga
        //        parola = "";
        //        bottoniDaColorare = new Button[lettere.GetLength(0), lettere.GetLength(1)];
        //        isFound = false;
        //    }
        //}
        //#endregion

        //#region << Cerca la parola da destra verso sinistra <<
        //void CercaParolaDxSx(string parolaDaCercare, string[,] lettere)
        //{
        //    bool isFound = false;
        //    string parola = "";
        //    Button[,] bottoniDaColorare = new Button[lettere.GetLength(0), lettere.GetLength(1)];
        //    int trovate = 0;

        //    for (int i = _matriceLettere.GetLength(0)-1; i >= 0; i--)
        //    {
        //        for (int j = _matriceLettere.GetLength(1)-1; j >= 0; j--)
        //        {
        //            CercaParola(
        //                i,
        //                j,
        //                ref isFound,
        //                ref parola,
        //                ref bottoniDaColorare,
        //                ref trovate,
        //                parolaDaCercare,
        //                _matriceLettere,
        //                COLORE_DXSX
        //                );
        //        }
        //        parola = "";
        //        bottoniDaColorare = new Button[lettere.GetLength(0), lettere.GetLength(1)];
        //        isFound = false;
        //    }




        //}
        //#endregion

        //#region ∧∧ Cerca la parola dal basso verso l'alto ∧∧
        //void CercaParolaUpDown(string parolaDaCercare, string[,] lettere)
        //{
        //    string parola = "";
        //    Button[,] bottoniDaColorare = new Button[lettere.GetLength(0), lettere.GetLength(1)];
        //    bool isFound = false;
        //    int trovate = 0;

        //    for (int i = 0; i < lettere.GetLength(1); i++)
        //    {
        //        for (int j = 0; j < lettere.GetLength(0); j++)
        //        {
        //            CercaParola(
        //                    j,
        //                    i,
        //                    ref isFound,
        //                    ref parola,
        //                    ref bottoniDaColorare,
        //                    ref trovate,
        //                    parolaDaCercare,
        //                    _matriceLettere,
        //                    COLORE_DXSX
        //                    );
        //        }
        //        parola = "";
        //        bottoniDaColorare = new Button[lettere.GetLength(0), lettere.GetLength(1)];
        //        isFound = false;
        //    }
        //}
        //#endregion

        //#region ∨∨ Cerca la parola dall'altro verso il basso ∨∨
        //void CercaParolaDownUp(string parolaDaCercare, string[,] lettere)
        //{
        //    string parola = "";
        //    Button[,] bottoniDaColorare = new Button[lettere.GetLength(0), lettere.GetLength(1)];
        //    bool isFound = false;
        //    int trovate = 0;


        //}
        //#endregion


        //#region Metodo generale per cercare parole

        //#region Metodo
        ///// <summary>
        ///// Cerca parole
        ///// Da usare in due for per girare una matrice
        ///// </summary>
        ///// <param name="i">Riga della matrice</param>
        ///// <param name="j">Colonna della matrice</param>
        ///// <param name="isFound">Se la parola sta venendo trovata</param>
        ///// <param name="parola">La parola presa dal puzzle carattere per carattere</param>
        ///// <param name="bottoniDaColorare">I bottoni da colorare</param>
        ///// <param name="parolaDaCercare">Parola da cercare</param>
        ///// <param name="lettere">Matrice con le lettere</param>
        //void CercaParola(int i, int j, ref bool isFound, ref string parola, ref Button[,] bottoniDaColorare, ref int trovate, string parolaDaCercare, string[,] lettere, SolidColorBrush color)
        //{
        //    trovate = 0;


        //    if (j == 10)
        //        Console.WriteLine("Eccoci");

        //    if (parolaDaCercare == "")
        //        throw new Exception("Non c'è nulla da cercare");

        //    if (parolaDaCercare[0].ToString() == lettere[i, j] || isFound)
        //    {
        //        isFound = true;
        //        parola += lettere[i, j];
        //        bottoniDaColorare[i, j] = _btns[i, j];
        //    }

        //    Console.WriteLine($"{parolaDaCercare} vs {parola}");

        //    if (parolaDaCercare.Substring(0, parola.Length) != parola)
        //    {
        //        // Resetta la ricerca della parola se è sbagliata
        //        parola = "";
        //        bottoniDaColorare = new Button[lettere.GetLength(0), lettere.GetLength(1)];
        //        isFound = false;
        //    }

        //    if (parolaDaCercare == parola)
        //    {
        //        ColoraBottoni(bottoniDaColorare, color);

        //        // Resetta la ricerca della parola se la trova
        //        trovate++;
        //        parola = "";
        //        bottoniDaColorare = new Button[lettere.GetLength(0), lettere.GetLength(1)];
        //        isFound = false;
        //    }
        //}
        //#endregion

        //#region Metodo colora bottoni
        //void ColoraBottoni(Button[,] bottoniDaColorare, SolidColorBrush color)
        //{
        //    for (int k = 0; k < bottoniDaColorare.GetLength(0); k++)
        //    {
        //        for (int l = 0; l < bottoniDaColorare.GetLength(1); l++)
        //        {
        //            if (bottoniDaColorare[k, l] != null)
        //            {
        //                bottoniDaColorare[k, l].Background = color;
        //            }
        //        }
        //    }
        //}
        //#endregion

        //#endregion

        #endregion
        
        /// <summary>
        /// Mostra una message box per notificare l'errore all'utente
        /// </summary>
        /// <param name="ex">Eccezione</param>
        private void MessaggioErrore(Exception ex)
        {
            MessageBox.Show($"Errore: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnCheckSolution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_btns == null)
                    throw new Exception("Non ho trovato nessuna matrice");
                for (int i = 0; i < _btns.GetLength(0); i++)
                {
                    for (int j = 0; j < _btns.GetLength(1); j++)
                    {
                        if (_btns[i, j].Background == COLORE_NUOVO)
                            _btns[i, j].Background = COLORE;
                        if (!(_btns[i, j].Background == COLORE))
                            _btns[i, j].Background = Brushes.LightBlue;
                    }
                }
            }
            catch(Exception ex)
            {
                MessaggioErrore(ex);
            }
        }
    }
}