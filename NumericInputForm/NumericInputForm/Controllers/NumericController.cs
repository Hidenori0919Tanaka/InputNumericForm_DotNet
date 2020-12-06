using NumericInputForm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NumericInputForm.Controllers
{
    public class NumericController : Controller
    {
        private const string sessionName_DateList = "DATA_LIST";
        private const string sessionName_Flag = "FLAG";

        // GET: Numeric
        public ActionResult Index()
        {
            string sessionFlag = null;
            if (Session[sessionName_Flag] != null)
            {
                sessionFlag = (string)Session[sessionName_Flag];
            }

            //セッション確認
            if (Session[sessionName_DateList] != null && Session[sessionName_Flag] == null)
            {
                //セッションをクリアにする
            }

            var dateList = (List<NumericModel>)Session[sessionName_DateList];

            //開始日指定なし　一件だけ入力がある　二件以上
            if (Session[sessionName_DateList] == null)
                dateList = CreateDateList(DateTime.UtcNow.AddHours(9));//単体テスト
            else if (dateList.Where(c => c.InputFlag).Count() > 1)//単体テスト
            {
                //起点 終点
                var startingPoint = dateList.OrderBy(o => o.Date).Where(x => x.InputFlag).First().Date;//単体テスト
                var endPoint = dateList.OrderBy(o => o.Date).Where(x => x.InputFlag).Last().Date;//単体テスト

                foreach (var model in dateList.OrderBy(o=>o.Date).SkipWhile(skip => skip.Date < startingPoint))
                {
                    if (model.Numeric == 0)
                        break;
                    else
                        startingPoint = model.Date;
                }

                //ループ
                while (startingPoint != endPoint)
                {
                    var distansX = 1;
                    //距離を測る
                    foreach (var model in dateList
                        .OrderBy(o => o.Date)
                        .SkipWhile(skip => skip.Date < startingPoint).Skip(1))
                    {
                        if (model.Numeric > 0)
                            break;
                        distansX++;
                    }

                    if (distansX == 1)
                    {
                        //例外
                    }

                    //補間の計算
                    var interponationModel = LinearInterpolationCulc(startingPoint, distansX, dateList);

                    //重複モデル削除
                    var removeModel = dateList.Where(r => r.Date == interponationModel.Date).First();
                    dateList.Remove(removeModel);

                    dateList.Add(interponationModel);
                    dateList.Sort((a, b) => a.Date.CompareTo(b.Date));
                    //次の起点設定
                    foreach (var model in dateList.OrderBy(o => o.Date).SkipWhile(skip => skip.Date < startingPoint))
                    {
                        if (model.Numeric == 0)
                            break;
                        else
                            startingPoint = model.Date;
                    }
                }
            }

            Session[sessionName_DateList] = dateList;
            return View();
        }

        private NumericModel LinearInterpolationCulc(DateTime startingPoint, int distansX, List<NumericModel> dateList)
        {
            //中間点 単体
            int x = (int)Math.Floor((decimal)distansX / 2);
            //補間値　単体
            var linearValue = dateList.Where(n => n.Date == startingPoint).FirstOrDefault().Numeric + (dateList.Where(n => n.Date == startingPoint.AddDays(distansX)).FirstOrDefault().Numeric - dateList.Where(n => n.Date == startingPoint).FirstOrDefault().Numeric) * ((float)x - 0) / (distansX - 0);
            //モデルに代入　単体
            var model = new NumericModel()
            {
                Date = startingPoint.AddDays(x),
                Numeric = linearValue,
                InputFlag = true,
                InterpolationFlag = true,
                OutputFlag = true
            };
            return model;
        }

        [HttpGet]
        public ActionResult SelectDate()
        {
            //
            var model = new NumericModel()
            {
                Date = DateTime.UtcNow.AddHours(9)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult SelectDate([Bind(Include = "Date")] NumericModel model)
        {
            //単体
            Session[sessionName_DateList] = CreateDateList(model.Date);
            Session[sessionName_Flag] = "SelectDate";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int year, int month, int day)
        {
            NumericModel nm = null;

            if (Session[sessionName_DateList] != null)
            {
                var sampleDataList = (List<NumericModel>)Session[sessionName_DateList];
                var targetDate = new DateTime(year, month, day);

                nm = sampleDataList.Find(x => x.Date == targetDate);
            }

            if (nm == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(nm);
            }
        }

        [HttpPost]
        public ActionResult Edit([Bind(Include = "Date, Numeric")] NumericModel model)
        {
            if (Session[sessionName_DateList] != null && ModelState.IsValid)
            {
                // セッション変数からデータを取得し、指定された日付のデータを入力された値に更新する
                var dataList = (List<NumericModel>)Session[sessionName_DateList];

                var d = dataList.Find(x => x.Date == model.Date);
                if (d != null && ModelState.IsValid)
                {
                    d.Numeric = model.Numeric;
                    d.InputFlag = true;
                    d.OutputFlag = true;
                    d.InterpolationFlag = false;
                }

                var clearList = dataList.OrderBy(o => o.Date).Where(i => i.InterpolationFlag).ToList();
                foreach (var m in clearList)
                {
                    var clearModel = m;
                    clearModel.Numeric = 0;
                    clearModel.InputFlag = false;
                    clearModel.InterpolationFlag = false;
                    clearModel.OutputFlag = false;
                    dataList.Remove(clearModel);
                    dataList.Add(clearModel);
                }
                //重複クリア
                var removeModel = dataList.Where(r => r.Date == model.Date).First();
                dataList.Remove(removeModel);
                dataList.Add(d);
                dataList.Sort((a, b) => a.Date.CompareTo(b.Date));
                // 更新されたデータを含むリストをセッション変数にセットする
                Session[sessionName_DateList] = dataList;
            }
            else if (!ModelState.IsValid)
            {
                return View(model);
            }

            return RedirectToAction("Index");
        }

        private List<NumericModel> CreateDateList(DateTime dt)
        {
            var dateList = new List<NumericModel>();

            //終わりの日時までループ 単体
            for (DateTime date = dt; date <= dt.AddYears(1).AddDays(-1); date = date.AddDays(1))
            {
                var model = new NumericModel()
                {
                    Date = date.Date,
                    InputFlag = false,
                    InterpolationFlag = false,
                    OutputFlag = false
                };
                dateList.Add(model);
            }
            //ソート　単体
            dateList.Sort((a, b) => a.Date.CompareTo(b.Date));
            return dateList;
        }
    }
}