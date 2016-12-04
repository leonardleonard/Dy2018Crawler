﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using Dy2018Crawler.Models;
using Pomelo.AspNetCore.TimedJob;

namespace Dy2018Crawler.Jobs
{
    public class AutoGetMovieListJob:Job
    {
        private static MovieInfoHelper latestMovieList = new MovieInfoHelper(Path.Combine(ConstsConf.WWWRootPath, "latestMovie.json"));

        private static MovieInfoHelper hotMovieList = new MovieInfoHelper(Path.Combine(ConstsConf.WWWRootPath, "hotMovie.json"));

        private static HtmlParser htmlParser = new HtmlParser();

        [Invoke(Begin = "2016-11-29 22:10", Interval = 1000 * 3600*3, SkipWhileExecuting =true)]
        public void Run()
        {
            LogHelper.Info("Start crawling");

            AddToLatestMovieList(100);
            AddToHotMovieList();
            LogHelper.Info("Finish crawling");
        }

        private void AddToLatestMovieList(int indexPageCount = 0)
        {
           
                try
                {
                    indexPageCount = indexPageCount == 0 ? 3 : indexPageCount;
                    //取前五页
                    for (var i = 1; i < indexPageCount; i++)
                    {
                        var index = i == 1 ? "" : "_" + i;
                        var indexURL = $"http://www.dy2018.com/html/gndy/dyzz/index{index}.html";
                        var htmlDoc = HTTPHelper.GetHTMLByURL(indexURL);
                        var dom = htmlParser.Parse(htmlDoc);
                        var lstDivInfo = dom.QuerySelectorAll("div.co_content8");
                        if (lstDivInfo != null)
                        {
                            lstDivInfo.FirstOrDefault().QuerySelectorAll("a").Where(a => a.GetAttribute("href").Contains("/i/")).ToList()
                            .ForEach(a =>
                            {
                                var onlineURL = "http://www.dy2018.com" + a.GetAttribute("href");
                                if (!latestMovieList.IsContainsMoive(onlineURL))
                                {
                                    MovieInfo movieInfo = MovieInfoHelper.GetMovieInfoFromOnlineURL(onlineURL);
                                    if (movieInfo.XunLeiDownLoadURLList != null && movieInfo.XunLeiDownLoadURLList.Count != 0)
                                        latestMovieList.AddToMovieDic(movieInfo);
                                }
                            });
                        }
                    }
                }
            catch (Exception ex)
            {
                LogHelper.Error("AddToLatestMovieList Exception", ex);
            }
           
        }

        private void AddToHotMovieList()
        {
           
                try
                {
                    var htmlDoc = HTTPHelper.GetHTMLByURL("http://www.dy2018.com/");
                    var dom = htmlParser.Parse(htmlDoc);
                    var lstDivInfo = dom.QuerySelectorAll("div.co_content222");
                    if (lstDivInfo != null)
                    {
                        //前三个DIV为新电影
                        foreach (var divInfo in lstDivInfo.Take(3))
                        {
                            divInfo.QuerySelectorAll("a").Where(a => a.GetAttribute("href").Contains("/i/")).ToList().ForEach(
                            a =>
                            {
                                var onlineURL = "http://www.dy2018.com" + a.GetAttribute("href");
                                if (!hotMovieList.IsContainsMoive(onlineURL))
                                {
                                    MovieInfo movieInfo = MovieInfoHelper.GetMovieInfoFromOnlineURL(onlineURL);
                                    if (movieInfo.XunLeiDownLoadURLList != null && movieInfo.XunLeiDownLoadURLList.Count != 0)
                                        hotMovieList.AddToMovieDic(movieInfo);
                                }
                            });
                        }
                    }

                }
                catch (Exception ex)
                {
                   LogHelper.Error("AddToHotMovieList",ex);
                }
          
        }

        
    }
}
