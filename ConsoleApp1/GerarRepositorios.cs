using System.Text;
using UtisEntityFramework;

namespace ConsoleApp1
{
    public static class GerarRepositorios
    {
        private class RepositorioFiles
        {
            public string Nome { get; set; }
            public string[] Linhas { get; set; }
        }

        public static void Gerar()
        {
            string[] arquivosEntidades = Directory.GetFiles("D:\\Repositorios\\Sysdam\\InspecaoWebAPI\\InspecaoWebAPI.DominioEntityFramework\\Entidades");
            const string ReplaceNomeEntidade = "_NOMEENTIDADE_";

            string RepositorioBaseTemplate = @"using InspecaoWebAPI.Aplicacao.RepositoriosEntityFramework;
using InspecaoWebAPI.DominioEntityFramework.Entidades;
using Sysdam.Database.DatabaseConnection;
using System.Linq.Expressions;

namespace InspecaoWebAPI.Infraestrutura.BancoDados.RepositoriosEntityFramework
{
    public class _NOMEENTIDADE_Repositorio : I_NOMEENTIDADE_Repositorio
    {
        private readonly IDatabaseConnection<_NOMEENTIDADE_> databaseConnection;

        public _NOMEENTIDADE_Repositorio(IDatabaseConnection<_NOMEENTIDADE_> databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        public _NOMEENTIDADE_ ObterPorId(params object[] ids) =>
             databaseConnection.ObterPorId(ids);

        public IQueryable<_NOMEENTIDADE_> Listar(Expression<Func<_NOMEENTIDADE_, bool>>? predicate = null, bool Tracking = false) =>
              databaseConnection.Listar(predicate, Tracking);

        public IQueryable<_NOMEENTIDADE_> ListarIgnorandoFiltros(Expression<Func<_NOMEENTIDADE_, bool>>? predicate = null, bool Tracking = false) =>
              databaseConnection.ListarIgnorandoFiltros(predicate, Tracking);

        public _NOMEENTIDADE_ Inserir(_NOMEENTIDADE_ model) =>
             databaseConnection.Inserir(model);

        public int Editar(_NOMEENTIDADE_ model) =>
            databaseConnection.Editar(model);

        public int Editar(_NOMEENTIDADE_ model, Expression<Func<_NOMEENTIDADE_, object>>[] properties) =>
            databaseConnection.Editar(model, properties);

        public int Deletar(_NOMEENTIDADE_ model) =>
            databaseConnection.Excluir(model);

        public int Deletar(params object[] ids) =>
            databaseConnection.Excluir(ids);

        public int EditarMultiplos(IEnumerable<_NOMEENTIDADE_> models, Expression<Func<_NOMEENTIDADE_, object>>[]? properties = null) =>
            databaseConnection.EditarMultiplos(models, properties);

        public int DeletarMultiplos(IEnumerable<_NOMEENTIDADE_> models) =>
            databaseConnection.DeletarMultiplos(models);

        public int InserirMultiplos(IEnumerable<_NOMEENTIDADE_> models) =>
            databaseConnection.InserirMultiplos(models);
    }
}";

            string RepositorioInterfaceTemplate = @"using InspecaoWebAPI.DominioEntityFramework.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace InspecaoWebAPI.Aplicacao.RepositoriosEntityFramework
{
    public interface I_NOMEENTIDADE_Repositorio
    {
        _NOMEENTIDADE_ ObterPorId(params object[] ids);

        IQueryable<_NOMEENTIDADE_> Listar(Expression<Func<_NOMEENTIDADE_, bool>>? predicate = null, bool Tracking = false);

        IQueryable<_NOMEENTIDADE_> ListarIgnorandoFiltros(Expression<Func<_NOMEENTIDADE_, bool>>? predicate = null, bool Tracking = false);

        _NOMEENTIDADE_ Inserir(_NOMEENTIDADE_ model);

        int Editar(_NOMEENTIDADE_ model);

        int Editar(_NOMEENTIDADE_ model, params Expression<Func<_NOMEENTIDADE_, object>>[] properties);

        int Deletar(_NOMEENTIDADE_ model);

        int Deletar(params object[] ids);

        int EditarMultiplos(IEnumerable<_NOMEENTIDADE_> models, params Expression<Func<_NOMEENTIDADE_, object>>[] properties);

        int DeletarMultiplos(IEnumerable<_NOMEENTIDADE_> models);

        int InserirMultiplos(IEnumerable<_NOMEENTIDADE_> models);
    }
}";

            List<RepositorioFiles> repositoriosFiles = new List<RepositorioFiles>();
            List<RepositorioFiles> repositoriosInterfaceFiles = new List<RepositorioFiles>();

            Console.WriteLine("services.AddScoped<IUnitOfWork, UnitOfWork>();");
            foreach (var entidade in arquivosEntidades)
            {
                string nomeEntidade = entidade.Split('\\').Last().Replace(".cs", "");
                string[] repositorio = RepositorioBaseTemplate.Replace(ReplaceNomeEntidade, nomeEntidade).Split('\n');
                string[] repositorioInterface = RepositorioInterfaceTemplate.Replace(ReplaceNomeEntidade, nomeEntidade).Split('\n');
                Console.WriteLine($"services.AddTransient<I{nomeEntidade}Repositorio, {nomeEntidade}Repositorio>();");

                repositoriosFiles.Add(new RepositorioFiles
                {
                    Nome = nomeEntidade,
                    Linhas = repositorio
                });

                repositoriosInterfaceFiles.Add(new RepositorioFiles
                {
                    Nome = nomeEntidade,
                    Linhas = repositorioInterface
                });
            }

            //Repositorio
            foreach (var repositorio in repositoriosFiles)
            {
                File.WriteAllLines($"D:\\Repositorios\\Sysdam\\InspecaoWebAPI\\InspecaoWebAPI.InfraestruturaEntityFramework\\BancoDados\\RepositoriosEntityFramework\\{repositorio.Nome}Repositorio.cs", repositorio.Linhas.Select(a => a.Replace("\r", "").Replace("\n", "")));
            }

            //Interface
            //UnitiOfWork
            StringBuilder stringBuilder = new StringBuilder("""
                using InspecaoWebAPI.Aplicacao.RepositoriosEntityFramework;
                using InspecaoWebAPI.DominioEntityFramework.Entidades;
                using InspecaoWebAPI.InfraestruturaEntityFramework.BancoDados.Entity.Contexto;
                using Microsoft.Extensions.Logging;
                using Sysdam.Database.DatabaseConnection;
                
                namespace InspecaoWebAPI.Infraestrutura.BancoDados.RepositoriosEntityFramework
                {
                    public class UnitOfWork : ControladorTransacao, IUnitOfWork
                    {
                        private readonly InspecaoContext context;

                """);
            foreach (var repositorioInterface in repositoriosInterfaceFiles)
            {
                File.WriteAllLines($"D:\\Repositorios\\Sysdam\\InspecaoWebAPI\\InspecaoWebAPI.Aplicacao\\RepositoriosEntityFramework\\I{repositorioInterface.Nome}Repositorio.cs", repositorioInterface.Linhas.Select(a => a.Replace("\r", "").Replace("\n", "")));
                stringBuilder.AppendLine($"        private I{repositorioInterface.Nome}Repositorio {repositorioInterface.Nome.ToCamelCase()};");
            }

            foreach(var repositorioInterface in repositoriosInterfaceFiles)
            {
                stringBuilder.AppendLine($"        public I{repositorioInterface.Nome}Repositorio {repositorioInterface.Nome} => {repositorioInterface.Nome.ToCamelCase()} ??= new {repositorioInterface.Nome}Repositorio(new DatabaseConnection<{repositorioInterface.Nome}>(loggerFactory, context));");
            }

            stringBuilder.AppendLine("""
                        
                        public UnitOfWork(InspecaoContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory) { }
                    }
                }
                """);

            File.WriteAllLines($"D:\\Repositorios\\Sysdam\\InspecaoWebAPI\\InspecaoWebAPI.InfraestruturaEntityFramework\\BancoDados\\RepositoriosEntityFramework\\UnitOfWork.cs", stringBuilder.ToString().Split('\n').Select(a => a.Replace("\r", "").Replace("\n", "")));
            
            //IUnitOfWork
            stringBuilder = new StringBuilder("""
                using Sysdam.Database.DatabaseConnection.Interface.Aplicacao;

                namespace InspecaoWebAPI.Aplicacao.RepositoriosEntityFramework
                {
                    public interface IUnitOfWork : IControladorTransacao
                    {

                """);

            foreach (var repositorioInterface in repositoriosInterfaceFiles)
            {
                stringBuilder.AppendLine($"        public I{repositorioInterface.Nome}Repositorio {repositorioInterface.Nome} {{ get; }}");
            }

            stringBuilder.AppendLine("""
                    }
                }
                """);

            File.WriteAllLines($"D:\\Repositorios\\Sysdam\\InspecaoWebAPI\\InspecaoWebAPI.Aplicacao\\RepositoriosEntityFramework\\IUnitOfWork.cs", stringBuilder.ToString().Split('\n').Select(a => a.Replace("\r", "").Replace("\n", "")));
        }
    }
}