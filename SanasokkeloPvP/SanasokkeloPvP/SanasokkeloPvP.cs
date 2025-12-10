using System;
using System.Collections.Generic;
using Jypeli;
using System.IO;
using System.Text;


/// @author kaipiip
/// @version 30.09.2025
/// <summary>
/// Sanasokkelo-peli, jossa tavoitteena on etsiä ruudukkoon arvottujen kirjainten seasta annetut seitsemän sanaa.
/// Pelaaja voi valita sanojen aihealueen kolmen eri teeman sisältä.
/// Ohjelma arpoo sanat valitusta teemasta, 50 sanan joukosta. Jos pelaaja ei valitse teemaa, ohjelma arpoo sen.
/// Kilpailuhenkeä peliin lisää kaksinpelin mahdollisuus.
/// </summary>
public class SanasokkeloPvP : Game
{
    // Haetaan sanat tekstitiedostoista:
    
    /// <summary>
    /// 50 sanan tekstitiedosto tekniikka-aiheisista sanoista.
    /// </summary>
    const string Teema1 = @"..\..\..\..\TeemaSanat\tekniikkaSanat.txt";
    /// <summary>
    /// 50 sanan tekstitiedosto tekstiiliaiheisista sanoista.
    /// </summary>
    const string Teema2 = @"..\..\..\..\TeemaSanat\tekstiiliSanat.txt";
    /// <summary>
    /// 50 sanan tekstitiedosto luontoaiheisista sanoista.
    /// </summary>
    const string Teema3 = @"..\..\..\..\TeemaSanat\luontoSanat.txt";
    
    // Sijoitetaan ne omiin taulukoihin (50 alkiota/teema):
    
    /// <summary>
    /// 50 alkioinen taulukko tekniikkasanoista.
    /// </summary>
    private string[] _tekniikkaSanat = File.ReadAllLines(Teema1);
    /// <summary>
    /// 50 alkioinen taulukko tekstiilisanoista.
    /// </summary>
    private string[] _tekstiiliSanat = File.ReadAllLines(Teema2);
    /// <summary>
    /// 50 alkioinen taulukko luontosanoista.
    /// </summary>
    private string[] _luontoSanat = File.ReadAllLines(Teema3);
    
    /// <summary>
    /// Label-oliotaulukko, johon sijoitetaan arvotut teemasanat.
    /// </summary>
    private Label[] _sanat = new Label[Sanamaara];
    /// <summary>
    /// Valikkojen taustakuva.
    /// </summary>
    private Image valikkoKuva = LoadImage("ValikkoRuutu");
    
    /// <summary>
    /// Totuusarvoinen muuttuja pelin teeman valinnalle.
    /// </summary>
    private bool _tekniikka;
    /// <summary>
    /// Totuusarvoinen muuttuja pelin teeman valinnalle.
    /// </summary>
    private bool _tekstiili;
    /// <summary>
    /// Totuusarvoinen muuttuja pelin teeman valinnalle.
    /// </summary>
    private bool _luonto;

    /// <summary>
    /// Totuusarvoinen muuttuja yksinpelin/kaksinpelin valinnalle.
    /// </summary>
    private bool _yksinpeli; // = true. kaksinpeli -> false.
    /// <summary>
    /// Pelaajan 1 peliolio.
    /// </summary>
    private GameObject _pelaaja1;
    /// <summary>
    /// Pelaajan 2 peliolio.
    /// </summary>
    private GameObject _pelaaja2;

    /// <summary>
    /// Ruudukonkoko, niin että Labelien väliin jää tilaa = ruudukko
    /// </summary>
    private const int Ruudunkoko = 40;
    /// <summary>
    /// Label-olion koko = ruutu.
    /// </summary>
    private const int Kirjainkoko = 38;
    /// <summary>
    /// Peliruudukon rivien määrä.
    /// </summary>
    private const int Rivit = 15;
    /// <summary>
    /// Peliruudukon sarakkeiden määrä.
    /// </summary>
    private const int Sarakkeet = 15;
    /// <summary>
    /// Etsittävien sanojen määrä.
    /// </summary>
    private const int Sanamaara = 7;
    /// <summary>
    /// Sommittelua varten muuttuja, jota käytetään useammissa aliohjelmissa.
    /// </summary>
    private const int Sijainti = 150;
    /// <summary>
    /// Label-oliomatriisi peliruudukolle, sisältää ruudut, kirjaimet ja etsittävät sanat ruudukossa.
    /// </summary>
    private Label[,] _kirjainRuudukko;
    
    /// <summary>
    /// Lista pelaajan 1 väreille. Sinisen sävyt.
    /// 0 -teksti,
    /// 1 -syvä sininen,
    /// 2 -tallennettu valinta/sana,
    /// 3 -aktiivinen valinta,
    /// 4 -läpinäkyvä
    /// </summary>
    private List<Color> _sininen = new List<Color>
    {
        new(0, 0, 66), // 0, teksti
        new(28, 28, 85), // 1, syvä sininen
        new(60, 60, 227), // 2, tallennettu valinta/sana
        new(154, 154, 248), // 3, aktiivinen valinta
        new(154, 154, 248, 100) // 4 läpinäkyvä
    };
    
    /// <summary>
    /// Lista pelaajan 2 väreille, vihreän eri sävyt.
    /// 0 -teksti,
    /// 1 -keskivihreä,
    /// 2 -tallennettu valinta/sana,
    /// 3 -aktiivinen valinta,
    /// 4 -läpinäkyvä.
    /// </summary>
    private List<Color> _vihrea = new List<Color>
    {
        new(23, 45, 0), // 0, teksti
        new(72, 146, 22), // 1, keskivihreä
        new( 124, 186, 0), // 2, tallennettu valinta/sana
        new(  154, 202, 34), // 3, aktiivinen valinta
        new( 154, 202, 34, 100) // 4 läpinäkyvä
    };
    
    /// <summary>
    /// Lista väreille valikoissa.
    /// 0 -perusväri,
    /// 1 -hovercolor,
    /// 2 -painettu
    /// </summary>
    private List<Color> _valikot = new List<Color>
    {
        new(232, 162, 46), // 0, perusväri
        new(236, 179, 69), // 1, hovercolor
        new(232, 139, 46), // 2, painettu
    };

    /// <summary>
    /// Rikottu musta sävy, käytetään lähinnä teksteissä.
    /// </summary>
    private Color _musta = new(14, 22, 9);
    /// <summary>
    /// Rikottu valkoinen sävy, Käytetään pohjavärinä ruudukolle ja valikoille.
    /// </summary>
    private Color _valkoinen = new(236, 236, 236);
    /// <summary>
    /// Rikottu ja läpinäkyvä valkoinen, käytetään osana sommittelua taustavärinä widgeteille.
    /// </summary>
    private Color _valkoinenLapiNakyva = new(236, 236, 236, 200);

    /// <summary>
    /// Fonttikoko 20
    /// </summary>
    private Font _pieni20 = new(20);
    /// <summary>
    /// Fonttikoko 22
    /// </summary>
    private Font _pieni22 = new(22);

    /// <summary>
    /// Pääohjelma
    /// </summary>
    public override void Begin()
    {
        IsFullScreen = true;
        
        LisaaTekstiRuudulle();
        Alkuvalikko();

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }
    

    /// <summary>
    /// Alkuvalikon määritys.
    /// </summary>
    public void Alkuvalikko()
    {
        Camera.ZoomToLevel(280);
        Level.Background.Image = valikkoKuva;
        _yksinpeli = true;

        MultiSelectWindow alkuvalikko = new MultiSelectWindow("SanasokkeloPvP",
            "Valitse aikakausilehti", "Yksinpeli: arvo lehti", "Kaksinpeli: arvo lehti", "Ohjeet", "Lopeta peli");
        alkuvalikko.AddItemHandler(0, ValitseTeema);
        alkuvalikko.AddItemHandler(1, AloitaPeli);
        alkuvalikko.AddItemHandler(2, Kaksinpeli);
        alkuvalikko.AddItemHandler(3, Peliohjeet);
        alkuvalikko.AddItemHandler(4, Exit);
        alkuvalikko.DefaultCancel = 4;
        alkuvalikko.CapturesMouse = false;
        alkuvalikko.Color = _valkoinenLapiNakyva;
        alkuvalikko.QuestionLabel.TextColor = _musta;
        alkuvalikko.SetButtonTextColor(_musta);
        PushButton[] painikkeet = alkuvalikko.Buttons;
        foreach (PushButton painike in painikkeet)
        {
            painike.Color = _valikot[0];
            painike.HoverColor = _valikot[1];
            painike.PressedColor = _valikot[2];
        }
        Add(alkuvalikko);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    
    /// <summary>
    /// Peliohjeet -sivu. Lisää ohjeet ruudulle sekä monivalintaikkunan takaisin valikkoon paluuta varten.
    /// </summary>
    public void Peliohjeet()
    {
        Camera.ZoomToLevel(280);
        Level.Background.Image = valikkoKuva;

        VerticalLayout padding = new VerticalLayout();
        padding.Spacing = 10;
        padding.TopPadding = 10;
        padding.BottomPadding = 10;
        padding.LeftPadding = 10;
        padding.RightPadding = 10;
        Widget ruutu = new Widget(padding);
        ruutu.Color = _valkoinenLapiNakyva;
        ruutu.Y = 100;
        Add(ruutu);

        List<string> ohjeet = new List<string>
        {
            "Pelin idea on etsiä annetut seitsemän sanaa kirjainten joukosta.",
            "Sanat voivat olla ristikossa normaalin lukusuunnan mukaan vaaka- tai pystysuunnassa.", "",
            "Pelaajan 1 ohjaimet: W, A, S, D ruudukossa liikkumiseen.",
            "Pidä Shift pohjassa ja liiku ruudukossa valitaksesi kirjaimia ruudukosta.", "",
            "Pelaajan 2 ohjaimet: nuolinäppäimet ruudukossa liikkumiseen.",
            "Pidä Välilyönti pohjassa ja liiku ruudukossa valitaksesi kirjaimia ruudukosta.",
            "", "Voittaja on enemmän sanoja löytänyt pelaaja."
        };
        foreach (string lause in ohjeet)
        {
            Label rivi = new Label(lause);
            rivi.TextColor = _musta;
            rivi.Font = _pieni20;
            ruutu.Add(rivi);
        }

        MultiSelectWindow palaa = new MultiSelectWindow("",
            "Palaa", "Lopeta peli");
        palaa.Y = -200;
        palaa.AddItemHandler(0, Alkuvalikko);
        palaa.AddItemHandler(0, PoistaTeksti(palaa, ruutu));
        palaa.AddItemHandler(1, Exit);
        palaa.DefaultCancel = 1;
        palaa.CapturesMouse = false;
        palaa.Color = _valkoinenLapiNakyva;
        palaa.QuestionLabel.TextColor = _musta;
        palaa.SetButtonTextColor(_musta);
        PushButton[] painikkeet = palaa.Buttons;
        foreach (PushButton painike in painikkeet)
        {
            painike.Color = _valikot[0];
            painike.HoverColor = _valikot[1];
            painike.PressedColor = _valikot[2];
        }
        Add(palaa);
    }

    
    /// <summary>
    /// Aliohjelma luo metodin, joka poistaa peliohjeet-ikkunassa luodun tekstiruudun peliohjeille.
    /// </summary>
    /// <param name="palaa">Monivalintaikkuna, jota kuunnellaan</param>
    /// <param name="ruutu">Widget, joka poistetaan</param>
    /// <returns>Paikallinen metodi, joka poistaa halutun widgetin kuuntelemalla monivalintaikkunaa.</returns>
    public Action PoistaTeksti(MultiSelectWindow palaa, Widget ruutu)
    {
        Action poista = Poista;

        void Poista()
        {
            if (palaa.SelectedIndex == 0) Remove(ruutu);
        }
        return poista;
    }

    
    /// <summary>
    /// Valikon määritys pelin teeman valinnalle.
    /// </summary>
    public void ValitseTeema()
    {
        Camera.ZoomToLevel(280);
        Level.Background.Image = valikkoKuva;
        
        MultiSelectWindow teemavalikko = new MultiSelectWindow("Valitse Aikakausilehti",
            "Tekniikkalehti", "Tekstiili ja Muoti -lehti", "Luontolehti", "Palaa", "Lopeta peli");
        teemavalikko.AddItemHandler(0, TeemanValinta(0));
        teemavalikko.AddItemHandler(1, TeemanValinta(1));
        teemavalikko.AddItemHandler(2, TeemanValinta(2));
        teemavalikko.AddItemHandler(3, Alkuvalikko);
        teemavalikko.AddItemHandler(4, Exit);
        teemavalikko.DefaultCancel = 4;
        teemavalikko.CapturesMouse = false;
        teemavalikko.Color = _valkoinenLapiNakyva;
        teemavalikko.QuestionLabel.TextColor = _musta;
        teemavalikko.SetButtonTextColor(_musta);
        PushButton[] painikkeet = teemavalikko.Buttons;
        foreach (PushButton painike in painikkeet)
        {
            painike.Color = _valikot[0];
            painike.HoverColor = _valikot[1];
            painike.PressedColor = _valikot[2];
        }
        Add(teemavalikko);
    }

    
    /// <summary>
    /// Pelaajan valinnan mukaan valittu muuttuja (_tekniikka, _tekstiili tai _luonto) saa arvon true,
    /// ja siirrytään valikkoon, jossa valitaan pelaajamäärä.
    /// </summary>
    public Action TeemanValinta(int nappi)
    {
        Action valinta = Valinta;

        void Valinta()
        {
            if (nappi == 0)
            {
                _tekniikka = true;
                PelaajaMaara();
            }
            if (nappi == 1)
            {
                _tekstiili = true;
                PelaajaMaara();
            }
            if (nappi == 2)
            {
                _luonto = true;
                PelaajaMaara();
            }
        }
        return valinta;
    }

    
    /// <summary>
    /// Valikon määritys pelaajamäärän valinnalle.
    /// </summary>
    public void PelaajaMaara()
    {
        Camera.ZoomToLevel(280);
        Level.Background.Image = valikkoKuva;

        MultiSelectWindow pelaajat = new MultiSelectWindow("Valitse Pelaajamäärä",
            "Gamma (yksinpeli)", "Gamma ja Myy (kaksinpeli)", "Palaa", "Lopeta peli");
        pelaajat.AddItemHandler(0, AloitaPeli);
        pelaajat.AddItemHandler(1, Kaksinpeli);
        pelaajat.AddItemHandler(2, ValitseTeema);
        pelaajat.AddItemHandler(3, Exit);
        pelaajat.DefaultCancel = 3;
        pelaajat.CapturesMouse = false;
        pelaajat.Color = _valkoinenLapiNakyva;
        pelaajat.QuestionLabel.TextColor = _musta;
        pelaajat.SetButtonTextColor(_musta);
        PushButton[] painikkeet = pelaajat.Buttons;
        foreach (PushButton painike in painikkeet)
        {
            painike.Color = _valikot[0];
            painike.HoverColor = _valikot[1];
            painike.PressedColor = _valikot[2];
        }
        Add(pelaajat);
    }

    
    /// <summary>
    /// Muuttaa yksinpelin arvon epätodeksi, ja aloittaa kaksinpelin.
    /// </summary>
    public void Kaksinpeli()
    {
        _yksinpeli = false;
        AloitaPeli();
    }

    
    /// <summary>
    /// Aloittaa pelin pelaajan tekemien valintojen perusteella.
    /// </summary>
    public void AloitaPeli()
    {
        Image taustakuva;
        if(_tekniikka) taustakuva = LoadImage("PeliRuutu-tekniikka");
        else if(_tekstiili) taustakuva = LoadImage("PeliRuutu-tekstiili");
        else if(_luonto) taustakuva = LoadImage("PeliRuutu-luonto");
        else taustakuva = valikkoKuva;
        
        Level.Background.Image = taustakuva;
        Camera.ZoomToLevel(280); // Sommittelun toimivuutta varten border size 280.
                                          // Pelioliot ja widgetit ei seuraa samoja sääntöjä. 
        
        Random r = new Random(); // Satunnaisuusolio, jota käytetään muutamassa aliohjelmassa.

        EtsittavatSanat();
        NaytaEtsittavatSanat();
        _kirjainRuudukko = LuoKirjainRuudukko(Rivit, Sarakkeet, r);
        LuoSommittelu();
        LisaaPelaajainfo();
        LisaaPelaajat();
        

        Keyboard.Listen(Key.W, ButtonState.Pressed, LiikutaPelaajaa, "Ylös", new Vector(0, Ruudunkoko), _pelaaja1);
        Keyboard.Listen(Key.A, ButtonState.Pressed, LiikutaPelaajaa, "Vasemmalle", new Vector(-Ruudunkoko, 0), _pelaaja1);
        Keyboard.Listen(Key.S, ButtonState.Pressed, LiikutaPelaajaa, "Alas", new Vector(0, -Ruudunkoko), _pelaaja1);
        Keyboard.Listen(Key.D, ButtonState.Pressed, LiikutaPelaajaa, "Oikealle", new Vector(Ruudunkoko, 0), _pelaaja1);
        Keyboard.Listen(Key.LeftShift, ButtonState.Down, AktiivinenValinta, "Pidä pohjassa valitaksesi kirjaimia", _pelaaja1);
        Keyboard.Listen(Key.LeftShift, ButtonState.Released, PelinKulku, null, _pelaaja1);

        if (!_yksinpeli)
        {
            Keyboard.Listen(Key.Up, ButtonState.Pressed, LiikutaPelaajaa, "Ylös", new Vector(0, Ruudunkoko), _pelaaja2);
            Keyboard.Listen(Key.Left, ButtonState.Pressed, LiikutaPelaajaa, "Vasemmalle", new Vector(-Ruudunkoko, 0), _pelaaja2);
            Keyboard.Listen(Key.Down, ButtonState.Pressed, LiikutaPelaajaa, "Alas", new Vector(0, -Ruudunkoko), _pelaaja2);
            Keyboard.Listen(Key.Right, ButtonState.Pressed, LiikutaPelaajaa, "Oikealle", new Vector(Ruudunkoko, 0), _pelaaja2);
            Keyboard.Listen(Key.Space, ButtonState.Down, AktiivinenValinta, "Pidä pohjassa valitaksesi kirjaimia", _pelaaja2);
            Keyboard.Listen(Key.Space, ButtonState.Released, PelinKulku, null, _pelaaja2);  
        }
        
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    
    /// <summary>
    /// Arpoo etsittävät sanat valitun teeman sisältä. Jos teemaa ei valita, se arvotaan.
    /// </summary>
    public void EtsittavatSanat()
    {
        if (_tekniikka) _sanat = ArvoTeemaSanat(_tekniikkaSanat);
        else if (_tekstiili) _sanat = ArvoTeemaSanat(_tekstiiliSanat);
        else if (_luonto) _sanat = ArvoTeemaSanat(_luontoSanat);
        else
        {
            _sanat = ArvoTeemaSanat(RandomGen.SelectOne(_tekniikkaSanat, _tekstiiliSanat, _luontoSanat));
        }
    }
    
    
    /// <summary>
    /// Arpoo valitun teeman sisältä 7 satunnaista sanaa 50 sanan joukosta.
    /// </summary>
    /// <param name="teema">Valittu teemasanasto.</param>
    /// <returns>Taulukko, jossa arvotut 7 sanaa Label -olioina</returns>
    public static Label[] ArvoTeemaSanat(string[] teema)
    {
        Label[] teemaSanat = new Label[Sanamaara];
        Random r = new Random();
        int[] lottorivi = new int[Sanamaara];

        for (int i = 0; i < lottorivi.Length; i++)
        {
            int j = 0;
            int lottonumero = r.Next(0, 50);
            while (j <= i)
            {
                if (lottonumero == lottorivi[j])
                {
                    j = 0;
                    lottonumero = r.Next(0, 50);
                }
                j++;
            }
            lottorivi[i] = lottonumero;
            teemaSanat[i] = new Label(teema[lottonumero].ToUpper());
        }
        return teemaSanat;
    }
    
    
    /// <summary>
    /// Näyttää mitkä sanat pelaajien tulee etsiä ristikosta, ja sijoittaa ne ruudulle widgettinä.
    /// </summary>
    public void NaytaEtsittavatSanat()
    {
        VerticalLayout pysty = new VerticalLayout();
        pysty.Spacing = 30;
        pysty.TopPadding = 10;
        pysty.BottomPadding = 10;
        pysty.LeftPadding = 10;
        pysty.RightPadding = 10;

        Widget s1 = new Widget(pysty);
        s1.X = Ruudunkoko * -11.5 + 2 + Sijainti;
        s1.Color = Color.Transparent;
        Add(s1);

        int leveys = 200;
        foreach (var sana in _sanat)
        {
            sana.Width = leveys;
            sana.Height = Kirjainkoko;
            sana.Font = Font.DefaultBold;
            sana.TextColor = _musta;
            s1.Add(sana);
        }
    }
    
    
    /// <summary>
    /// Liikuttaa pelaajaa. Jos pelaajan sijainti on ruudukon reunalla, pelaaja ei pääse liikkumaan ulos ruudukosta.
    /// </summary>
    /// <param name="vektori">Vektori, jonka mukaan pelaaja liikkuu</param>
    /// <param name="pelaaja">Pelaaja 1 tai 2</param>
    public void LiikutaPelaajaa(Vector vektori, GameObject pelaaja)
    {
        if ((int)pelaaja.Y == 280 && vektori == new Vector(0, Ruudunkoko)) // Yläreuna
        {
            vektori = new Vector(0, 0);
        }

        if ((int)pelaaja.Y == -280 && vektori == new Vector(0, -Ruudunkoko)) // Alareuna
        {
            vektori = new Vector(0, 0);
        }

        if ((int)pelaaja.X == -280 + Sijainti && vektori == new Vector(-Ruudunkoko, 0)) // Vasen reuna
        {
            vektori = new Vector(0, 0);
        }

        if ((int)pelaaja.X == 280 + Sijainti && vektori == new Vector(Ruudunkoko, 0)) // Oikea reuna
        {
            vektori = new Vector(0, 0);
        }
        pelaaja.Move(vektori);
    }

    
    /// <summary>
    /// Lisää pelaajan/pelaajat pelikentälle.
    /// </summary>
    public void LisaaPelaajat()
    {
        _pelaaja1 = new GameObject(Ruudunkoko, Ruudunkoko, Shape.Rectangle);
        _pelaaja1.Color = _sininen[4];
        _pelaaja1.X = -7 * Ruudunkoko + Sijainti;
        _pelaaja1.Y = 7 * Ruudunkoko;
        Add(_pelaaja1, 3);

        if (!_yksinpeli)
        {
            _pelaaja2 = new GameObject(Kirjainkoko, Kirjainkoko, Shape.Rectangle);
            _pelaaja2.Color = _vihrea[4];
            _pelaaja2.X = 7 * Ruudunkoko + Sijainti;
            _pelaaja2.Y = 7 * Ruudunkoko;
            Add(_pelaaja2, 3);
        }
    }

    
    /// <summary>
    /// Lisää ruudulle tekstin, joka selittää pelin tarinan.
    /// </summary>
    public void LisaaTekstiRuudulle()
    {
        VerticalLayout pysty = new VerticalLayout();
        pysty.Spacing = 5;
        pysty.TopPadding = 10;
        pysty.BottomPadding = 10;
        pysty.LeftPadding = 5;
        pysty.RightPadding = 5;

        Widget tekstit = new Widget(pysty);
        tekstit.Width = Kirjainkoko * Sarakkeet * 1.5;
        tekstit.Color = _valkoinenLapiNakyva;
        tekstit.Y = Ruudunkoko * 10;
        Add(tekstit);

        List<string> tarina = new List<string>
        {
            "Ulkoavaruuden oliot Gamma ja Myy löysivät avaruudesta elinvoimaisen sinisen planeetan.",
            "Soluttautuakseen planeetan asukkaiden joukkoon heidän täytyy oppia lukemaan näiden merkintöjä", 
            "Erilaiset paperiniput herättivät heidän mielenkiintonsa, ja he päättivät opetella",
            "havaitsemaan niistä sanoja merkkisopan seasta."
        };

        foreach (string lause in tarina)
        {
            Label rivi = new Label(Kirjainkoko * Sarakkeet * 1.555, Kirjainkoko, lause);
            rivi.TextColor = _musta;
            rivi.Font = _pieni22;
            tekstit.Add(rivi);
        }
    }

    
    /// <summary>
    /// Lisää ruudulle peliruudukon sivuille visuaalisiksi elementeiksi pari laatikkoa tukemaan sommittelua.
    /// </summary>
    public void LuoSommittelu()
    {
        Widget sanalaatikko = new Widget(Ruudunkoko * 7, Ruudunkoko * Rivit);
        sanalaatikko.Color = _valkoinenLapiNakyva;
        sanalaatikko.X = Ruudunkoko * -11.5 + 2 + Sijainti;
        Add(sanalaatikko);

        Widget infolaatikko = new Widget(Ruudunkoko * Sarakkeet * 1.5, Ruudunkoko * 3.5);
        infolaatikko.Color = _valkoinenLapiNakyva;
        infolaatikko.Y = Ruudunkoko * -9.5;
        infolaatikko.X = Sijainti - 4 * Ruudunkoko + 11;
        Add(infolaatikko);
    }

    
    /// <summary>
    /// Lisää ruudulle pelaajan/pelaajien värit ja ohjaimet tukemaan pelinkulun ymmärrettävyyttä.
    /// </summary>
    void LisaaPelaajainfo()
    {
        HorizontalLayout vaaka = new HorizontalLayout();
        vaaka.Spacing = 160; // = 600 - (2*200 + 2*10 + 2*10). Peliruudukon leveys - label leveysx2 - paddingit x2.
        vaaka.TopPadding = 10;
        vaaka.BottomPadding = 10;
        vaaka.LeftPadding = 10;
        vaaka.RightPadding = 10;

        VerticalLayout pysty = new VerticalLayout();
        pysty.Spacing = 10;
        pysty.TopPadding = 10;
        pysty.BottomPadding = 10;
        pysty.LeftPadding = 10;
        pysty.RightPadding = 10;

        Widget pelaajainfo = new Widget(pysty);
        pelaajainfo.Y =
            -360; // = -(600 + 2*10 + 2*Ruudunkoko + 2*10)/2. Peliruudukon korkeus - Label korkeus x2 - paddingit x2.
        pelaajainfo.X = Sijainti;
        pelaajainfo.Color = Color.Transparent;
        Add(pelaajainfo);

        Widget pelaajat = new Widget(vaaka);
        pelaajat.Width = Kirjainkoko * Sarakkeet;
        pelaajat.Color = Color.Transparent;
        pelaajainfo.Add(pelaajat);

        Label pelaaja1 = new Label(200, Ruudunkoko * 2, "Gamma" + '\n' + "Pelaaja 1: Pinkki"
                                                        + '\n' + "W, A, S, D, ja Shift");
        pelaaja1.TextColor = _sininen[0];
        pelaaja1.Color = _sininen[2];
        pelaaja1.Font = _pieni20;
        pelaajat.Add(pelaaja1);

        if (!_yksinpeli)
        {
            Label pelaaja2 = new Label(230, Ruudunkoko * 2, "Myy" + '\n' + "Pelaaja 2: Vihreä"
                                                            + '\n' + "nuolinäppäimet ja Välilyönti");
            pelaaja2.TextColor = _vihrea[0];
            pelaaja2.Color = _vihrea[2];
            pelaaja2.Font = _pieni20;
            pelaajat.Add(pelaaja2);
        }
    }
    
    
    /// <summary>
    /// Luo peliruudukon kokoisen char-matrsiisin, ja sijoittaa etsittävien sanojen kirjaimet siihen
    /// normaalissa lukusuunnassa joko vaaka- tai pystysuuntaan.
    /// </summary>
    /// <param name="r">Satunnaisuus-olio</param>
    /// <returns>Muuten tyhjä matriisi, mutta etsittävien sanojen kirjaimet sijoitettuna.</returns>
    public char[,] SanatRuudukkoon(Random r)
    {
        char[,] sanatRuudukossa = new char[Rivit, Sarakkeet];
        int rivit = sanatRuudukossa.GetLength(0);
        int sarakkeet = sanatRuudukossa.GetLength(1);

        for(int k = 0; k < _sanat.Length; k++) // Suorita jokaiselle sanalle:
        {
            string kirjain;
            if (k < 4) // vaakasuuntaan
            // Yritin ensin r.NextDouble, mutta satunnaisuus oli niin satunnaista, että suurinosa ajasta sanat olivat
            // aina pystysuunnassa. Päätin sitoa sanojen pysty/vaaka -suhteen 3/4,
            // jotta peliruudukko olisi monipuolisempi.
            {
                while (true)
                {
                    int rivi = r.Next(0, rivit);
                    int sarake = r.Next(0, sarakkeet);
                    bool sijoita = true;

                    while (sarakkeet - sarake < _sanat[k].Text.Length) sarake = r.Next(0, sarakkeet);
                    for (int j = 0; j < _sanat[k].Text.Length; j++)
                    {
                        kirjain = _sanat[k].Text.Substring(j, 1);
                        if (sanatRuudukossa[rivi, sarake + j] != '\0')
                        {
                            if (sanatRuudukossa[rivi, sarake + j] != kirjain.ToCharArray()[0]) sijoita = false;
                        }
                    }

                    if (!sijoita) continue;
                    for (int i = 0; i < _sanat[k].Text.Length; i++)
                    {
                        kirjain = _sanat[k].Text.Substring(i, 1);
                        sanatRuudukossa[rivi, sarake + i] = kirjain.ToCharArray()[0];
                    }

                    break;
                } 
            }
            else // pystysuuntaan sama
            {
                while (true)
                {
                    int rivi = r.Next(0, rivit);
                    int sarake = r.Next(0, sarakkeet);
                    bool sijoita = true;

                    while (rivit - rivi < _sanat[k].Text.Length) rivi = r.Next(0, rivit);
                    for (int j = 0; j < _sanat[k].Text.Length; j++)
                    {
                        kirjain = _sanat[k].Text.Substring(j, 1);
                        if (sanatRuudukossa[rivi + j, sarake] != '\0')
                        {
                            if (sanatRuudukossa[rivi + j, sarake] != kirjain.ToCharArray()[0]) sijoita = false;
                        }
                    }

                    if (!sijoita) continue;
                    for (int i = 0; i < _sanat[k].Text.Length; i++)
                    {
                        kirjain = _sanat[k].Text.Substring(i, 1);
                        sanatRuudukossa[rivi + i, sarake] = kirjain.ToCharArray()[0];
                    }
                    break;
                }
            }
        }
        return sanatRuudukossa;
    }

    
    /// <summary>
    /// Luo tekstiruudun (Label-olio) johon sijoitetaan kirjain matriisista.
    /// </summary>
    /// <param name="ruutuX">Ruudun sijainnin x-koordinaatti.</param>
    /// <param name="ruutuY">Ruudun sijainnin y-koordinaatti.</param>
    /// <param name="merkki">Kirjain, joka sijoitetaan ruutuun</param>
    /// <returns>Label -olio, jota käytetään peliruudukon luonnissa.</returns>
    public Label Kirjain(int ruutuX, int ruutuY, char merkki)
    {
        Label kirjain = new Label(Kirjainkoko, Kirjainkoko, merkki.ToString());
        kirjain.X = ruutuX;
        kirjain.Y = ruutuY;
        kirjain.TextColor = _musta;
        kirjain.Color = _valkoinen;
        Add(kirjain, -3);
        return kirjain;
    }

    
    /// <summary>
    /// Muuttaa pelaajan valitseman ruudun väriä ja antaa ruudulle valintaa identifioivan tägin.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jonka  valintaa kuunnellaan</param>
    public void AktiivinenValinta(GameObject pelaaja)
    {
        foreach (var ruutu in _kirjainRuudukko)
        {
            if ((int)ruutu.X == (int)pelaaja.X && (int)ruutu.Y == (int)pelaaja.Y) // Näissä ei pitäisi olla liukulukuja.
            {
                if (pelaaja == _pelaaja1)
                {
                    ruutu.Tag = "p1";
                    if (ruutu.Color != _valkoinen) continue;
                    ruutu.Color = _sininen[3];
                }
                if (pelaaja == _pelaaja2)
                {
                    ruutu.Tag = "p2";
                    if (ruutu.Color != _valkoinen) continue;
                    ruutu.Color = _vihrea[3];
                }
            }
        }
    }
    
    
    /// <summary>
    /// Aliohjelma seuraa pelin kulkua.
    /// Se vertaa pelaajan valitsemaa merkkijonoa etsittäviin sanoihin.
    /// Jos merkkijono ei vastaa etsittävää sanaa, palautetaan ruudun värit oletusväreihin.
    /// Jos merkkijono vastaa etsittävää sanaa, muutetaan ruudun
    /// sekä sanaluettelossa olevan sanan värit pelaajan väreihin.
    /// Kun kaikki etsittävät sanat on löydetty, aukeaa ikkuna jossa ilmoitetaan, kuka löysi eniten sanoja ja
    /// tarjotaan pelin uudelleen aloittamista/lopettamista.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jonka valintaa kuunnellaan.</param>
    public void PelinKulku(GameObject pelaaja)
    {
        List<Label> valittuMerkkijono = new List<Label>();
        StringBuilder valinta = new StringBuilder();
        bool sanaLoydetty = false;
        int sanatGamma = 0;
        int sanatMyy = 0;
        int sanaLaskuri = 0;
        
        foreach (var ruutu in _kirjainRuudukko)
        {
            if (ruutu.Tag.ToString() == "p1" && pelaaja == _pelaaja1)
            {
                valittuMerkkijono.Add(ruutu);
            }
            if (ruutu.Tag.ToString() == "p2" && pelaaja == _pelaaja2)
            {
                valittuMerkkijono.Add(ruutu);
            }
        }
        
        foreach (var ruutu in valittuMerkkijono)
        {
            valinta.Append(ruutu.Text);
            if(pelaaja == _pelaaja1) ruutu.Tag = "valintaP1";
            if(pelaaja == _pelaaja2) ruutu.Tag = "valintaP2";
        }

        string merkkijono = valinta.ToString();
        
        for (int i = 0; i < _sanat.Length; i++)
        {
            if (merkkijono.Equals(_sanat[i].Text) && pelaaja == _pelaaja1)
            {
                sanaLoydetty = true;
                _sanat[i].TextColor = _sininen[2];
            }
            if (merkkijono.Equals(_sanat[i].Text) && pelaaja == _pelaaja2)
            {
                sanaLoydetty = true;
                _sanat[i].TextColor = _vihrea[2];
            }
        }

        if (sanaLoydetty)
        {
            foreach (var ruutu in valittuMerkkijono)
            {
                if (ruutu.Tag.ToString() == "valintaP1" && pelaaja == _pelaaja1)
                {
                    ruutu.TextColor = _sininen[0];
                    ruutu.Color = _sininen[2];
                }
                if (ruutu.Tag.ToString() == "valintaP2" && pelaaja == _pelaaja2)
                {
                    ruutu.TextColor = _vihrea[0];
                    ruutu.Color = _vihrea[2];
                }
            }
        }
        else
        {
            foreach (var ruutu in valittuMerkkijono)
            {
                if (ruutu.TextColor != _musta) continue;
                ruutu.Color = _valkoinen;
            }
        }
        
        foreach (var sana in _sanat)
        {
            if(sana.TextColor != _musta) sanaLaskuri++;
            if (sana.TextColor == _sininen[2])
            {
                
                ++sanatGamma;
            }
            if (sana.TextColor == _vihrea[2])
            {
                ++sanatMyy;
            }
        }
        
        if (sanaLaskuri == Sanamaara)
        {
            if (_yksinpeli)
            {
                MultiSelectWindow loppu = new MultiSelectWindow("Löysit kaikki sanat!",
                    "Aloita alusta", "Lopeta peli");
                loppu.X = Sijainti;
                loppu.AddItemHandler(0, AloitaAlusta);
                loppu.AddItemHandler(1, Exit);
                loppu.DefaultCancel = 1;
                loppu.CapturesMouse = false;
                loppu.Color = _valkoinen;
                loppu.QuestionLabel.TextColor = _musta;
                loppu.SetButtonTextColor(_musta);
                PushButton[] painikkeet = loppu.Buttons;
                foreach (PushButton painike in painikkeet)
                {
                    painike.Color = _sininen[2];
                    painike.HoverColor = _sininen[3];
                    painike.PressedColor = _sininen[1];
                }
                Add(loppu, 3);
            }
            else if (sanatGamma > sanatMyy)
            {
                MultiSelectWindow loppu = new MultiSelectWindow("Gamma löysi enemmän sanoja!",
                    "Aloita alusta", "Lopeta peli");
                loppu.X = Sijainti;
                loppu.AddItemHandler(0, AloitaAlusta);
                loppu.AddItemHandler(1, Exit);
                loppu.DefaultCancel = 1;
                loppu.CapturesMouse = false;
                loppu.Color = _valkoinen;
                loppu.QuestionLabel.TextColor = _musta;
                loppu.SetButtonTextColor(_musta);
                PushButton[] painikkeet = loppu.Buttons;
                foreach (PushButton painike in painikkeet)
                {
                    painike.Color = _sininen[2];
                    painike.HoverColor = _sininen[3];
                    painike.PressedColor = _sininen[1];
                }
                Add(loppu, 3);
            }
            else
            {
                MultiSelectWindow loppu = new MultiSelectWindow("Myy löysi enemmän sanoja!",
                    "Aloita alusta", "Lopeta peli");
                loppu.X = Sijainti;
                loppu.AddItemHandler(0, AloitaAlusta);
                loppu.AddItemHandler(1, Exit);
                loppu.DefaultCancel = 1;
                loppu.CapturesMouse = false;
                loppu.Color = _valkoinen;
                loppu.QuestionLabel.TextColor = _musta;
                loppu.SetButtonTextColor(_musta);
                PushButton[] painikkeet = loppu.Buttons;
                foreach (PushButton painike in painikkeet)
                {
                    painike.Color = _vihrea[2];
                    painike.HoverColor = _vihrea[3];
                    painike.PressedColor = _vihrea[1];
                }
                Add(loppu, 3);
            }
        }
    }
    
    
    /// <summary>
    /// Luo matriisin Label- olioista, joka toimii pelin kirjainruudukkona.
    /// </summary>
    /// <param name="a">Peliruudukon rivien määrä.</param>
    /// <param name="b">Peliruudukon sarakkeiden määrä.</param>
    /// <param name="r">Satunnaisuus-olio.</param>
    /// <returns>Kirjainruudukko.</returns>
    public Label[,] LuoKirjainRuudukko(int a, int b, Random r)
    {
        Label[,] kirjainRuudukko = new Label[a, b];
        int rivit = kirjainRuudukko.GetLength(0);
        int sarakkeet = kirjainRuudukko.GetLength(1);
        char[,] kirjaimet = ArvoKirjaimet(SanatRuudukkoon(r), r);

        for (int rivi = 0; rivi < rivit; rivi++)
        {
            for (int sarake = 0; sarake < sarakkeet; sarake++)
            {
                kirjainRuudukko[rivi, sarake] = Kirjain(Ruudunkoko * (-Rivit / 2) + sarake * Ruudunkoko + Sijainti,
                    Ruudunkoko * (Sarakkeet / 2) + rivi * -1 * Ruudunkoko, kirjaimet[rivi, sarake]);
                // Ruudukon koko 600x600, eli -300 --> 300 (plus sijainti oikealle 150), jolloin ensimmaisen ruudun KESKIPISTE
                // on paikassa x = -300 + (Ruudunkoko/2) + sijainti(150) = -130. y = 280.
                // Atribuuteilla laskettuna esim:
                // Ruudukonkoko = 15 --> 15/2 = 7,5.
                // Koska atribuutit kokonaislukuja 7,5 = 7.<-- Ensimmäisen ruudun keskipiste.
                // Attribuuteilla laskettuna rivien ja sarakkeiden määrä voi olla vain pariton -> 15, 17, 19, ...
                // Seuraavan ruudun keskipiste on Ruudunkoon(40) päässä edellisestä.
            }
        }
        return kirjainRuudukko;
    }

    
    /// <summary>
    /// Aliohjelma arpoo ja sijoittaa kirjaimet halutun peliruudukon kokoisen matriisin tyhjiin ruutuihin.
    /// Suuret kirjaimet arvotaan ASCII koodia hyödyntäen. Ei sisällä ääkkösiä.
    /// </summary>
    /// <param name="sanatRuudukossa">Peliruudukon kokoinen matriisi, jonka ruutuihin on sijoitettu etsittävien
    /// sanojen kirjaimet.</param>
    /// <param name="r">Satunnaisuus-olio.</param>
    /// <returns>Matriisi, jonka alkiot ovat etsittävien sanojen krjaimet,
    /// sekä arvottuja suuria kirjaimia täyttämässä ruudut ilman sanaa.</returns>
    public char[,] ArvoKirjaimet(char[,] sanatRuudukossa, Random r)
    {
        int rivit = sanatRuudukossa.GetLength(0);
        int sarakkeet = sanatRuudukossa.GetLength(1);

        for (int rivi = 0; rivi < rivit; rivi++)
        {
            for (int sarake = 0; sarake < sarakkeet; sarake++)
            {
                if (sanatRuudukossa[rivi, sarake] != '\0') continue;
                int ascii = r.Next(65, 91); // ASCII 65-90 A-Z (ei ääkösiä)
                sanatRuudukossa[rivi, sarake] = (char)ascii;
            }
        }
        return sanatRuudukossa;
    }

    
    /// <summary>
    /// Tyhjentää pelikentän ja aloittaa pelin alusta.
    /// </summary>
    public void AloitaAlusta()
    {
        ClearAll();
        
        // Nollataan teeman valinnan totuusarvot.
        _tekniikka = false;
        _tekstiili = false;
        _luonto = false;
        
        LisaaTekstiRuudulle();
        Alkuvalikko();
    }
}