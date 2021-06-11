using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.Master.Payment;
using OPS.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OPS.Business.Repository
{
    public class MstPaymentRepository :GenericRepository<MstPayment>, IMstPaymentRepository<MstPayment>
    {
        public MstPaymentRepository(OpsContext context) : base(context)
        {

        }

        public AjaxResponseModel Create(MstPayment payment, string userID)
        {
            var entity = new MstPayment
            {
                Code = payment.Code.Trim(),
                Name = payment.Name.Trim(),
                NameKn = payment.NameKn.Trim(),
                Category = payment.Category.Trim(),
                InsertUserId = userID,
                InsertDate = DateTime.Now,
                IsDeleted = false
            };
            Add(entity);
            return new AjaxResponseModel(true, null);
        }

        public AjaxResponseModel Delete(string cd, string userID)
        {
            var query = _context.MstPayments
                        .Include(q => q.Contracts)
                        .FirstOrDefault(c => c.Cd.ToString() == cd);
            if(query.Contracts.Any(c => !c.IsDeleted && c.IsCompleted))
                return new AjaxResponseModel(false, "この得意先が使われている為、削除できません。");
            
            query.IsDeleted = true;
            query.UpdateUserId = userID;
            query.UpdateDate = DateTime.Now;
            Update(query);
            return new AjaxResponseModel(true, null);
        }

        public PaymentViewModel Load(PaymentViewModel filtering)
        {
            filtering.Keyword = filtering.Keyword?.Trim().ToLower();
            filtering.keywordKana = filtering.keywordKana?.Trim().ToLower();

            var result = new PaymentViewModel();

            var payment = _context.MstPayments.Where(q => !q.IsDeleted);

            if (!string.IsNullOrEmpty(filtering.Keyword))
            {
                payment = payment.Where(q => q.Code.ToLower().Contains(filtering.Keyword)
                                        || q.Category.ToLower().Contains(filtering.Keyword)
                                        || q.Name.ToLower().Contains(filtering.Keyword)
                                        || q.NameKn.Contains(filtering.Keyword)
                                        || q.NameKn.Contains(filtering.keywordKana));
            }

            result.TotalRowsAfterFiltering = payment.Count();
            payment = Filtering(payment, filtering);

            result.Payments = payment
                .Select(q => new
                {
                    q.Cd,
                    q.Category,
                    q.Code,
                    q.Name,
                    q.NameKn
                })
                .AsEnumerable()
                .Select(q => new PaymentModel()
                {
                    Cd = q.Cd,
                    Category = q.Category,
                    Code = q.Code,
                    Name = q.Name,
                    NameKn = q.NameKn
                })
                .ToList();

            return result;
        }

        private IQueryable<MstPayment> Filtering(IQueryable<MstPayment> payments, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "Category":
                    if (filtering.SortDirection == "asc")
                    {
                        payments = payments.OrderBy(x => x.Category).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        payments = payments.OrderByDescending(x => x.Category).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "Code":
                    if (filtering.SortDirection == "asc")
                    {
                        payments = payments.OrderBy(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        payments = payments.OrderByDescending(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "Name":
                    if (filtering.SortDirection == "asc")
                    {
                        payments = payments.OrderBy(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        payments = payments.OrderByDescending(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "NameKn":
                    if (filtering.SortDirection == "asc")
                    {
                        payments = payments.OrderBy(x => x.NameKn).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        payments = payments.OrderByDescending(x => x.NameKn).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                default:
                    if (filtering.SortDirection == "asc")
                    {
                        payments = payments.OrderBy(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        payments = payments.OrderByDescending(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
            }
            return payments;
        }
        public  AjaxResponseModel Update(MstPayment payment, string cd, string userID)
        {
            var query = _context.MstPayments.FirstOrDefault(c => c.Cd.ToString() == cd);
            if (query == null)
            {
                return new AjaxResponseModel
                {
                    Status = false,
                    Message = "この得意先管理が存在していません。"
                };
            }

            query.UpdateDate = DateTime.Now;
            query.Code = payment.Code.Trim();
            query.Category = payment.Category.Trim();
            query.UpdateUserId = userID;
            query.Name = payment.Name.Trim();
            query.NameKn = payment.NameKn.Trim();
            Update(query);
            return new AjaxResponseModel(true, null);
        }

        public bool IsDuplicated(string Code,string Category, string cd)
        {
            var query = _context.MstPayments.FirstOrDefault(c => c.Code.ToLower().Equals(Code.Trim().ToLower())
                                                               && c.Category.ToLower().Equals(Category.ToLower().Trim())
                                                               && !c.Cd.ToString().Equals(cd));
            return query != null;
        }

        public AjaxResponseModel ImportCSV(IFormFile file, string userId)
        {
            string[] headers = new string[] { "支払先コード", "支払先区分", "支払先略称名", "支払先かな名" };
            var result = new AjaxResponseModel(false, "");
            try
            {
                string fileExtension = Path.GetExtension(file.FileName);

                if (fileExtension.ToUpper() != ".CSV")
                {
                    result.Message = "アプロードファイルはCSVのみ指定ください。";
                    return result;
                }

                var payments = new List<ImportedPaymentModel>();
                var errorRecords = new CustomLookup();
                var isBadRecord = false;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.GetEncoding(932)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<ImportedPaymentModelMap>();
                    csv.Configuration.TrimOptions = TrimOptions.Trim | TrimOptions.InsideQuotes;
                    csv.Configuration.BadDataFound = context =>
                    {
                        if (context.Row > 1)
                        {
                            isBadRecord = true;
                            var field = context.RawRecord.Replace("\r\n", "");
                            var message = string.Format("「{0}」はCSVフォーマットが正しくありません。", field.Length > 25 ? $"{field.Substring(0, 25)}..." : field);
                            errorRecords.Add(context.Row, message);
                        }
                    };

                    csv.Read();
                    csv.ReadHeader();
                    string[] headerRow = csv.Context.HeaderRecord;

                    if (headerRow.Length != headers.Length)
                    {
                        var message = $"以下の{headers.Length}列のCSVファイルを選択してください。\n";
                        message += string.Join("\n", headers.Select(h => $"「{h}」"));

                        result.Message = message;
                        return result;
                    }

                    while (csv.Read())
                    {
                        if (!isBadRecord)
                        {
                            if (csv.Context.Record.Length != headers.Length)
                            {
                                errorRecords.Add(csv.Context.Row, "カラム数があっていません。");
                            }

                            var validationResults = new List<ValidationResult>();
                            var payment = csv.GetRecord<ImportedPaymentModel>();
                            var context = new ValidationContext(payment, null, null);
                            var isValid = Validator.TryValidateObject(payment, context, validationResults, true);

                            if (isValid)
                            {
                                payments.Add(payment);
                            }
                            else
                            {
                                foreach (var valResult in validationResults)
                                {
                                    errorRecords.Add(csv.Context.Row, valResult.ErrorMessage);
                                }
                            }
                        }

                        isBadRecord = false;
                    }
                }

                if (errorRecords.Count > 0)
                {
                    var message = "データが無効です。\n";
                    message += string.Join("\n", errorRecords.Select(a => $"{a.Key}行目・{a.Value}"));
                    message += "\n再度確認してください。";

                    result.Message = message;
                    return result;
                }

                if (payments.Count > 0)
                {
                    //check duplicates
                    var duplicateDic = payments.GroupBy(p => new { p.Code, p.Category })
                                                .Where(p => p.Count() > 1)
                                                .ToDictionary(p => $"{p.Key.Code}・{p.Key.Category}", p => p.Count());

                    if (duplicateDic.Count > 0)
                    {
                        var message = "支払先コード・支払先区分は以下の値が重複しています。\n";
                        message += string.Join("\n", duplicateDic.Select(p => $"「{p.Key}」（{p.Value}回）"));
                        message += "\n再度確認してください。";

                        result.Message = message;
                        return result;
                    }

                    var now = DateTime.Now;
                    var newPaymentList = new List<MstPayment>();
                    foreach (var csvPayment in payments)
                    {
                        var paymentInDb = _context.MstPayments.FirstOrDefault(p => p.Code == csvPayment.Code && p.Category == csvPayment.Category);
                        if (paymentInDb != null)
                        {
                            paymentInDb.Name = csvPayment.Name;
                            paymentInDb.NameKn = csvPayment.NameKana;
                            paymentInDb.UpdateUserId = userId;
                            paymentInDb.UpdateDate = now;
                        }
                        else
                        {
                            var newPayment = new MstPayment
                            {
                                Code = csvPayment.Code,
                                Category = csvPayment.Category,
                                Name = csvPayment.Name,
                                NameKn = csvPayment.NameKana,
                                InsertUserId = userId,
                                InsertDate = now,
                                UpdateUserId = userId,
                                UpdateDate = now,
                            };

                            newPaymentList.Add(newPayment);
                        }
                    }

                    if (newPaymentList.Count > 0)
                    {
                        _context.MstPayments.AddRange(newPaymentList);
                    }

                    _context.SaveChanges();

                    result.Status = true;
                    return result;
                }
                else
                {
                    result.Message = "CSVファイルにデータがありません。再度確認してください。";
                    return result;
                }
            }
            catch
            {
                result.Status = false;
                result.Message = "エラーが発生しました。もう一度お試しください。";
                return result;
            }
        }
    }
}
