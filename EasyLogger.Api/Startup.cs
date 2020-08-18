using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyLogger.Api.EasyTools;
using EasyLogger.Api.Model;
using EasyLogger.DbStorage.Interface;
using EasyLogger.SqlSugarDbStorage;
using EasyLogger.SqlSugarDbStorage.Impl;
using EasyLogger.SqlSugarDbStorage.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;
using SqlSugarProvider = EasyLogger.SqlSugarDbStorage.Impl.SqlSugarProvider;

namespace EasyLogger.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            #region SqlSugar
            var defaultDbPath = Path.Combine(PathExtenstions.GetApplicationCurrentPath(), $"{Configuration["EasyLogger:DbName"]}.db");

            services.AddSingleton<ISqlSugarProvider>(new SqlSugarProvider(new SqlSugarSetting()
            {

                Name = SqlSugarDbStorageConsts.DefaultProviderName,
                ConnectionString = @$"Data Source={defaultDbPath}",
                DatabaseType = DbType.Sqlite,
                LogExecuting = (sql, pars) =>
                {
                    Console.WriteLine($"sql:{sql}");
                }

            }));
   
            services.AddTransient(typeof(ISqlSugarRepository<,>), typeof(SqlSugarRepository<,>));
            services.AddTransient(typeof(IDbRepository<,>), typeof(SqlSugarRepository<,>));
            services.AddSingleton<ISqlSugarProviderStorage, DefaultSqlSugarProviderStorage>();
            #endregion

            #region Ĭ�ϴ����������ݿ� ��  ʱ�����ݿ�

            if (!File.Exists(defaultDbPath))
            {
                var db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = @$"Data Source={defaultDbPath}",
                    DbType = DbType.Sqlite,
                    IsAutoCloseConnection = true, // �Զ��ͷ������������������������������ͷ�
                    InitKeyType = InitKeyType.Attribute// ��ʵ�������ж�ȡ������������Ϣ
                });

                db.CodeFirst.BackupTable().InitTables<EasyLoggerProject>();

                db.Dispose();
            }
            #endregion


            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            //app.Use(async (context, next) =>
            //{
            //    var sqlStorage = app.ApplicationServices.GetService<ISqlSugarProviderStorage>();
            //    var sugarClient = sqlStorage.GetByName(null, SqlSugarDbStorageConsts.DefaultProviderName).Sugar;
            //    Console.WriteLine("�鿴sugarClient");
            //});

         



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}