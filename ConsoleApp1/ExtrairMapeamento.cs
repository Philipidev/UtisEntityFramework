using System.Text;

namespace ConsoleApp1
{
    public static class ExtrairMapeamento
    {
        private class MapFiles
        {
            public string NomeArquivo { get; set; }
            public string[] Linhas { get; set; }
        }


        public static void Extrair()
        {
            string[] contextoLinhas = File.ReadAllLines("D:\\Repositorios\\Sysdam\\InspecaoWebAPI\\InspecaoWebAPI.InfraestruturaEntityFramework\\BancoDados\\Entity\\Contexto\\InspecaoContext.cs");
            string caminhoParaMapeamento = "D:\\Repositorios\\Sysdam\\InspecaoWebAPI\\InspecaoWebAPI.InfraestruturaEntityFramework\\BancoDados\\Entity\\Mapeamento\\";

            bool append = false;

            List<MapFiles> mapFiles = new List<MapFiles>();

            StringBuilder? stringBuilder = null;
            string? nomeEntidade = null;
            
            //Criar lista de arquivos de mapeamento
            for (int i = 0; i < contextoLinhas.Length; i++)
            {
                string linha = contextoLinhas[i];
                if (linha.StartsWith("            modelBuilder.Entity<"))
                {
                    append = true;
                    nomeEntidade = linha.Replace("            modelBuilder.Entity<", "").Split(">").First();
                    stringBuilder = new StringBuilder($$"""
                        using InspecaoWebAPI.DominioEntityFramework.Entidades;
                        using Microsoft.EntityFrameworkCore;
                        using Microsoft.EntityFrameworkCore.Metadata.Builders;

                        namespace InspecaoWebAPI.InfraestruturaEntityFramework.BancoDados.Entity.Mapeamento
                        {
                            public class {{nomeEntidade}}Map : IEntityTypeConfiguration<{{nomeEntidade}}>
                            {
                                public void Configure(EntityTypeBuilder<{{nomeEntidade}}> builder)

                        """);
                    continue;
                }

                if (linha.StartsWith("            });"))
                {
                    append = false;
                    stringBuilder.AppendLine("""
                                }
                            }
                        }
                        """);
                    mapFiles.Add(new MapFiles
                    {
                        NomeArquivo = $"{nomeEntidade}Map",
                        Linhas = stringBuilder.ToString().Split("\n")
                    });
                    continue;
                }

                if (append)
                {
                    stringBuilder.AppendLine(linha.Replace("entity.", "builder."));
                }
            }


            //Gerar arquivos de mapeamento
            foreach (var mapFile in mapFiles)
            {
                File.WriteAllLines($"{caminhoParaMapeamento}{mapFile.NomeArquivo}.cs", mapFile.Linhas.Select(a => a.Replace("\r", "").Replace("\n", "")));
            }


            //Adicionar arquivos de mapeamento no contexto
            append = true;
            stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using InspecaoWebAPI.InfraestruturaEntityFramework.BancoDados.Entity.Mapeamento;");
            for (int i = 0; i < contextoLinhas.Length; i++)
            {
                var linha = contextoLinhas[i];
                if (linha.StartsWith("        protected override void OnModelCreating(ModelBuilder modelBuilder)"))
                {
                    append = false;
                    stringBuilder.AppendLine("""
                        protected override void OnModelCreating(ModelBuilder modelBuilder)
                        {
                            modelBuilder.Model.GetEntityTypes()
                                     .Where(entityType => typeof(IExclusaoLogica).IsAssignableFrom(entityType.ClrType))
                                     .ToList()
                                     .ForEach(entityType => GetType()
                                          .GetMethod(nameof(ConfigureSoftDelete), BindingFlags.NonPublic | BindingFlags.Static)
                                          .MakeGenericMethod(entityType.ClrType)
                                          .Invoke(null, new object[] { modelBuilder })
                                     );
                        """);
                    foreach (var mapFile in mapFiles)
                    {
                        stringBuilder.AppendLine($"            modelBuilder.ApplyConfiguration(new {mapFile.NomeArquivo}());");
                    }
                    stringBuilder.AppendLine("""
                                    
                                    OnModelCreatingPartial(modelBuilder);
                                }

                                partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
                            }
                        }
                        """);
                }

                if (append)
                {
                    stringBuilder.AppendLine(linha);
                }
            }

            File.WriteAllLines("D:\\Repositorios\\Sysdam\\InspecaoWebAPI\\InspecaoWebAPI.InfraestruturaEntityFramework\\BancoDados\\Entity\\Contexto\\InspecaoContext.cs", stringBuilder.ToString().Split("\n").Select(a => a.Replace("\r", "").Replace("\n", "")));
        }
    }
}