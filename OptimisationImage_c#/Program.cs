using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

class ImageOptimizer
{
    static readonly int[] resolutions = { 1080, 720, 480 };

    // IMPORTANT : MODIFIEZ CE CHEMIN pour votre dossier d'images réel. 
    static readonly string sourceDir = "C:\\Users\\masaracousti\\Desktop\\bordel\\IPI\\GenerationImageOpti";

    static readonly string outputDir = Path.Combine(sourceDir, "output");

    static void Main()
    {
        Console.WriteLine($"Dossier source: {sourceDir}");

        Directory.CreateDirectory(outputDir);
        Console.WriteLine($"Dossier de sortie créé/vérifié: {outputDir}");

        var sequentialTime = OptimizeImagesSequential();
        var parallelTime = OptimizeImagesParallel();

        WriteReadme(sequentialTime, parallelTime);

        Console.WriteLine("\n*** Optimisation terminée. ***");
        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }

    /// Optimise les images séquentiellement.
    static long OptimizeImagesSequential()
    {
        Console.WriteLine("\n--- Démarrage de l'optimisation SÉQUENTIELLE ---");
        var sw = Stopwatch.StartNew();

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            if (!IsImageFile(file)) continue;

            try
            {
                ProcessImage(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur sur {file} (Séquentiel): {ex.Message}");
            }
        }

        sw.Stop();
        Console.WriteLine($"[Séquentiel] Durée totale : {sw.ElapsedMilliseconds} ms");
        return sw.ElapsedMilliseconds;
    }

    /// Optimise les images en parallèle via Parallel.ForEach.
    static long OptimizeImagesParallel()
    {
        Console.WriteLine("\n--- Démarrage de l'optimisation PARALLÉLISÉE ---");
        var sw = Stopwatch.StartNew();

        var files = Directory.GetFiles(sourceDir);
        Parallel.ForEach(files, file =>
        {
            if (!IsImageFile(file)) return;

            try
            {
                ProcessImage(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur sur {file} (Parallèle): {ex.Message}");
            }
        });

        sw.Stop();
        Console.WriteLine($"[Parallélisé] Durée totale : {sw.ElapsedMilliseconds} ms");
        return sw.ElapsedMilliseconds;
    }

    /// Vérifie si le fichier est une image (JPG, JPEG, PNG).
    static bool IsImageFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".jpg" || ext == ".jpeg" || ext == ".png";
    }

    /// Charge une image, la redimensionne aux résolutions cibles, et sauvegarde les résultats.
    static void ProcessImage(string file)
    {
        using (var image = Image.Load(file))
        {
            foreach (var res in resolutions)
            {
                int width = image.Width * res / image.Height;

                using var resized = image.Clone(ctx => ctx.Resize(width, res));

                var filename = Path.GetFileNameWithoutExtension(file);
                var ext = Path.GetExtension(file);
                var outputPath = Path.Combine(outputDir, $"{filename}_{res}p{ext}");

                resized.Save(outputPath);
            }
        }
    }

    /// Écrit le fichier README.md avec les temps de performance.
    static void WriteReadme(long seqMs, long parMs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Résultats d'optimisation des images");
        sb.AppendLine();
        sb.AppendLine($"| Version | Durée (ms) |");
        sb.AppendLine($"| :--- | :--- |");
        sb.AppendLine($"| Séquentielle | **{seqMs}** |");
        sb.AppendLine($"| Parallélisée | **{parMs}** |");
        sb.AppendLine();
        sb.AppendLine("## Conclusion");
        sb.AppendLine(seqMs > parMs
            ?"Succès du Parallélisme : Le parallélisme a amélioré significativement la vitesse d'exécution."
            :"Analyse Requise : Les performances parallèles ne sont pas meilleures ou sont similaires.");

        var readmePath = Path.Combine(sourceDir, "README.md");
        File.WriteAllText(readmePath, sb.ToString());
        Console.WriteLine($"\nFichier README.md créé ici: {readmePath}");
    }
}