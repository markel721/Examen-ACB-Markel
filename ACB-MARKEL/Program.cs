// Cada partido es un array de textos con estos campos:
// p[0]=jornada, p[1]=equipo local, p[2]=equipo visitante, p[3]=resultado
// p[4]=mvp, p[5]=puntos, p[6]=asistencias, p[7]=rebotes, p[8]=equipo del mvp
 
// Leer el CSV una sola vez, ruta: ../../../acb2526.csv
Console.Write("Ruta del CSV: ");
string rutaCSV = Console.ReadLine().Trim();

if (!File.Exists(rutaCSV)){
    Console.WriteLine("El CSV no existe");
    return;
}

// Carpeta donde se guardan todos los ficheros
string carpetaFicheros = "../../../Ficheros/";

string[] lineasCSV = File.ReadAllLines(rutaCSV);
List<string[]> partidos = new List<string[]>();
 
for (int i = 1; i < lineasCSV.Length; i++){
    partidos.Add(SepararLinea(lineasCSV[i]));
}
// Menu principal 
int menu = 0;
 
while (menu != 6){
 
    Console.WriteLine("\n MENU");
    Console.WriteLine("1. Resultados de un equipo");
    Console.WriteLine("2. Resultados de una jornada");
    Console.WriteLine("3. Top 3 MVPS");
    Console.WriteLine("4. Clasificacion");
    Console.WriteLine("5. Crear un fichero por equipo");
    Console.WriteLine("6. Salir");
    Console.Write("Opcion: ");
 
    if (!int.TryParse(Console.ReadLine(), out menu)){
        Console.WriteLine("Opcion incorrecta");
        continue;
    }
 
    try{
        if (menu == 1) VerResultadosEquipo();
        else if (menu == 2) VerResultadosJornada();
        else if (menu == 3) VerTopMvps();
        else if (menu == 4) VerClasificacion();
        else if (menu == 5) CrearFicherosPorEquipo();
        else if (menu != 6) Console.WriteLine("Opcion incorrecta");
    }
    catch (Exception e){
        Console.WriteLine("Error: " + e.Message);
    }
}
 
// OPCIONES DEL MENU
 
// Opcion 1: todos los resultados de un equipo
void VerResultadosEquipo(){
    Console.Write("Equipo(): ");
    MostrarYGuardar(LineasDeEquipo(Console.ReadLine().Trim()));
}

// Opcion 2: todos los partidos de una jornada
void VerResultadosJornada(){
    Console.Write("Numero de jornada: ");
    string jornada = "Jornada " + Convert.ToInt32(Console.ReadLine());
 
    List<string> lineas = new List<string>();
 
    foreach (string[] p in partidos){ 
        if (p[0] == jornada)
            lineas.Add(PasarATexto(p));
    }
 
    MostrarYGuardar(lineas);
}
 
// Opcion 3: los 3 jugadores que mas veces han sido MVP
void VerTopMvps()
{
    // En la posicion i esta el jugador y las veces que ha sido MVP
    List<string> jugadores = new List<string>();
    List<int> veces = new List<int>();
 
    foreach (string[] p in partidos){
        int posicion = BuscarPosicion(jugadores, p[4]);
 
        if (posicion == -1){
            jugadores.Add(p[4]);
            veces.Add(1);
        }
        else{
            veces[posicion]++;
        }
    }
 
    List<string> lineas = new List<string>();
 
    // Buscamos el mayor 3 veces, al elegirlo ponemos sus veces a -1 para no repetirlo
    for (int puesto = 1; puesto <= 3; puesto++){
        int mejor = 0;
 
        for (int i = 1; i < jugadores.Count; i++){
            if (veces[i] > veces[mejor])
                mejor = i;
        }
 
        lineas.Add(puesto + ". " + jugadores[mejor] + " - " + veces[mejor] + " veces MVP");
        veces[mejor] = -1;
    }
 
    MostrarYGuardar(lineas);
}
 
// Opcion 4: clasificacion 
void VerClasificacion(){
    List<string> nombres = ObtenerEquipos();
    List<int[]> stats = new List<int[]>();
 
    for (int i = 0; i < nombres.Count; i++)
        stats.Add(new int[4]);
 
    foreach (string[] p in partidos){
        int[] local = stats[BuscarPosicion(nombres, p[1])];
        int[] visitante = stats[BuscarPosicion(nombres, p[2])];
 
        // El resultado viene como "79-69" (local-visitante)
        string[] marcador = p[3].Split('-');
        int puntosLocal = Convert.ToInt32(marcador[0]);
        int puntosVisitante = Convert.ToInt32(marcador[1]);
 
        local[2] += puntosLocal;
        local[3] += puntosVisitante;
        visitante[2] += puntosVisitante;
        visitante[3] += puntosLocal;
 
        if (puntosLocal > puntosVisitante){
            local[0]++;
            visitante[1]++;
        }
        else{
            visitante[0]++;
            local[1]++;
        }
    }
 
    //Comparando nombres y estadisticas
    for (int i = 0; i < nombres.Count - 1; i++){
        for (int j = 0; j < nombres.Count - 1 - i; j++){
            int[] actual = stats[j];
            int[] siguiente = stats[j + 1];
            
            //[0]	victorias, [1]	derrotas
            //[2]	puntos a favor, [3]	puntos en contra
           
            bool siguienteEsMejor = siguiente[0] > actual[0]
                || (siguiente[0] == actual[0] && siguiente[2] - siguiente[3] > actual[2] - actual[3]);
 
            if (siguienteEsMejor){
                string nombreAuxiliar = nombres[j];
                nombres[j] = nombres[j + 1];
                nombres[j + 1] = nombreAuxiliar;
 
                stats[j] = siguiente;
                stats[j + 1] = actual;
            }
        }
    }
 
    List<string> lineas = new List<string>();
 
    for (int i = 0; i < nombres.Count; i++){
        int[] s = stats[i];
        lineas.Add((i + 1) + ". " + nombres[i] + " - " + s[0] + "V " + s[1] + "D"
                   + " (diferencia de puntos: " + (s[2] - s[3]) + ")");
    }
 
    MostrarYGuardar(lineas);
}
 
// Opcion 5: un .txt por equipo dentro de la carpeta Ficheros
void CrearFicherosPorEquipo(){
    foreach (string equipo in ObtenerEquipos()){
        File.WriteAllLines(carpetaFicheros + equipo + ".txt", LineasDeEquipo(equipo));
    }
 
    Console.WriteLine("Ficheros creados en la carpeta Ficheros");
}
 
// METODOS  
// Convierte un partido en una linea de texto para mostrar o guardar
string PasarATexto(string[] p){
    return p[0] + " | " + p[1] + " " + p[3] + " " + p[2]
         + " | MVP: " + p[4] + " (" + p[8] + ") - "
         + p[5] + " pts, " + p[6] + " asist., " + p[7] + " reb.";
}
 
// Lineas de todos los partidos (como local o visitante) de un equipo
List<string> LineasDeEquipo(string equipo){
    List<string> lineas = new List<string>();
 
    foreach (string[] p in partidos){
        if (p[1].ToLower() == equipo.ToLower() || p[2].ToLower() == equipo.ToLower())
            lineas.Add(PasarATexto(p));
    }
 
    return lineas;
}
 
// Nombres de los equipos sin repetir
List<string> ObtenerEquipos(){
    List<string> equipos = new List<string>();
 
    foreach (string[] p in partidos){
        if (!equipos.Contains(p[1]))
            equipos.Add(p[1]);
 
        if (!equipos.Contains(p[2]))
            equipos.Add(p[2]);
    }
 
    return equipos;
}
 
// Devuelve la posicion de un texto dentro de una lista, o -1 si no esta
int BuscarPosicion(List<string> lista, string texto){
    for (int i = 0; i < lista.Count; i++){
        if (lista[i] == texto)
            return i;
    }
 
    return -1;
}
 
// Muestra las lineas por pantalla y pregunta si se quieren guardar en un .txt 
void MostrarYGuardar(List<string> lineas){
    if (lineas.Count == 0){
        Console.WriteLine("No hay resultados");
        return;
    }
 
    Console.WriteLine();
 
    foreach (string linea in lineas)
        Console.WriteLine(linea);
 
    Console.Write("\nGuardar en un fichero? (s/n): ");
    if (Console.ReadLine().Trim().ToLower() != "s")
        return;
 
    Console.Write("Nombre del fichero (sin .txt): ");
    string nombre = Console.ReadLine().Trim();
 
    File.WriteAllLines(carpetaFicheros + nombre + ".txt", lineas);
    Console.WriteLine("Fichero guardado: " + carpetaFicheros + nombre + ".txt");
}

// Separa una linea del CSV en campos ignorando las comas que van entre comillas
string[] SepararLinea(string linea){
    List<string> campos = new List<string>();
    string campo = "";
    bool entreComillas = false;
 
    foreach (char c in linea){
        if (c == '"'){
            entreComillas = !entreComillas;
        }
        else if (c == ',' && !entreComillas){
            campos.Add(campo);
            campo = "";
        }
        else{
            campo += c;
        }
    }
 
    campos.Add(campo);
    return campos.ToArray();
}