using Microsoft.AspNetCore.Mvc.Rendering;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Utility;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.Master.Event;
using OPS.ViewModels.User.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using static OPS.Utility.Constants;

namespace OPS.Business.Repository
{
    public class MasterDataRepository : IMasterDataRepository
    {
        protected readonly OpsContext _context;

        public MasterDataRepository(OpsContext context)
        {
            _context = context;
        }

        public List<SelectListItem> GetSelectListAgeRange()
        {
            var lstAgeRangeEnum = CommonExtension.ToEnumSelectList<AgeRange>();
            List<SelectListItem> lstAgeRange = new List<SelectListItem>();

            foreach (var item in lstAgeRangeEnum)
            {
                SelectListItem ageRangeItem = new SelectListItem();
                ageRangeItem.Value = item.Value;
                ageRangeItem.Text = item.Name;
                lstAgeRange.Add(ageRangeItem);
            }

            return lstAgeRange;
        }

        public List<SelectListItem> GetSelectListCareer()
        {
            var result = _context.MstCareers.Where(c => !c.IsDeleted).Select(c => new
            {
                c.Cd,
                c.Name,
                c.Code,
            })
            .AsEnumerable()
            .OrderBy(c => c.Code)
            .Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Cd.ToString(),
            }).ToList();


            return result;
        }

        public List<SelectListItem> GetSelectListMedia(bool isNullItemFirst = false)
        {
            var result = _context.MstMedias.Where(m => !m.IsDeleted).Select(m => new
            {
                m.Cd,
                m.Name,
                m.Code
            })
            .AsEnumerable()
            .OrderBy(m => m.Code)
            .Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Cd.ToString(),
            }).ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListSalesDepartment(bool isNullItemFirst = false)
        {
            var lstDepartment = CommonExtension.ToEnumSelectList<SalesDepartment>()
                                .Select(d => new SelectListItem
                                {
                                    Value = d.Value,
                                    Text = d.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                lstDepartment.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return lstDepartment;
        }

        public List<SelectListItem> GetSelectListEventSalesman(string userId, bool isNullItemFirst = false)
        {
            var result = _context.EventSalesAssigments
                        .Where(a => a.Event.ApplicationUserId == userId && !a.Salesman.IsDeleted)
                        .Select(a => new
                        {
                            a.SalesmanCd,
                            a.Salesman.Name,
                            a.Salesman.Code,
                        })
                        .AsEnumerable()
                        .OrderBy(s => s.Code)
                        .Select(s => new SelectListItem
                        {
                            Value = s.SalesmanCd.ToString(),
                            Text = string.Format("{0} - {1}", s.Code, s.Name),
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListOrganization(bool isNullItemFirst = false)
        {
            var result = _context.MstOrganizations
                        .Where(o => !o.IsDeleted)
                        .Select(o => new
                        {
                            o.Cd,
                            o.Name,
                            o.Code,
                        })
                        .AsEnumerable()
                        .OrderBy(o => o.Code)
                        .Select(s => new SelectListItem
                        {
                            Value = s.Cd.ToString(),
                            Text = s.Name,
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListInputter(bool isNullItemFirst = false)
        {
            var result = CommonExtension.ToEnumSelectList<Inputter>()
                                .Select(d => new SelectListItem
                                {
                                    Value = d.Value,
                                    Text = d.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListArtist(bool isNullItemFirst = false)
        {
            var result = _context.MstArtists
                        .Where(a => !a.IsDeleted)
                        .Select(a => new
                        {
                            a.Cd,
                            a.Name,
                            a.Code,
                        })
                        .AsEnumerable()
                        .OrderBy(a => a.Code)
                        .Select(a => new SelectListItem
                        {
                            Value = a.Cd.ToString(),
                            Text = a.Name,
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return result;
        }

        public List<SelectListItem> GetListArtistByDepartmentAndProduct(string productName = "", int? departmentCd = null)
        {
            var artists = _context.MstArtists.Where(a => !a.IsDeleted);

            //var department = _context.MstDepartments.Find(departmentCd);
            //var department_name = "";

            //if (department != null)
            //{
            //    department_name = department.Name;
            //}

            if (departmentCd.HasValue)
            {
                artists = artists.Where(a => a.ArtistDepartments.Any(ad => ad.DepartmentCd == departmentCd || ad.Department.Name == Constants.Department_Const));
            }

            //if (department_name == Constants.Department_Const)
            //{
            //    artists = _context.MstArtists.Where(a => !a.IsDeleted);
            //}

            if (!string.IsNullOrEmpty(productName))
            {
                artists = artists.Where(a => a.Products.Any(p => !p.IsDeleted && p.OriginalName.ToUpper().StartsWith(productName.ToUpper().Trim())));
            }

            var result = artists
                        .Select(a => new
                        {
                            a.Cd,
                            a.Name,
                            a.Code,
                        })
                        .AsEnumerable()
                        .OrderBy(a => a.Code)
                        .Select(a => new SelectListItem
                        {
                            Value = a.Cd.ToString(),
                            Text = a.Name,
                        })
                        .ToList();

            return result;
        }

        public List<ProductSelectListItem> GetListProductByDepartmentAndArtist(string artistName = "", int? departmentCd = null)
        {
            var products = _context.MstProducts.Where(p => !p.IsDeleted);
            //var department = _context.MstDepartments.Find(departmentCd);
            //var department_name = "";

            //if (department !=null )
            //{
            //    department_name = department.Name;
            //}

            if (departmentCd.HasValue)
            {
                products = products.Where(p => p.Artist.ArtistDepartments.Any(ad => ad.DepartmentCd == departmentCd || ad.Department.Name == Constants.Department_Const));
            }

            //if (department_name == Constants.Department_Const)
            //{
            //    products = _context.MstProducts.Where(p => !p.IsDeleted);
            //}

            if (!string.IsNullOrEmpty(artistName))
            {
                products = products.Where(p => p.Artist.Name.ToUpper().StartsWith(artistName.ToUpper().Trim()));
            }

            var result = products
                        .Select(p => new
                        {
                            p.Cd,
                            p.OriginalName,
                            p.Code,
                        })
                        .AsEnumerable()
                        .OrderBy(p => p.Code)
                        .Select(p => new ProductSelectListItem
                        {
                            Cd = p.Cd.ToString(),
                            Name = p.OriginalName,
                        })
                        .ToList();

            return result;
        }

        public List<SelectListItem> GetSelectListDeliveryTime(bool isNullItemFirst = false)
        {
            var result = CommonExtension.ToEnumSelectList<DeliveryTime>()
                                .Select(d => new SelectListItem
                                {
                                    Value = d.Value,
                                    Text = d.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListDeliveryPlace(bool isNullItemFirst = false)
        {
            var result = CommonExtension.ToEnumSelectList<DeliveryPlace>()
                                .Select(d => new SelectListItem
                                {
                                    Value = d.Value,
                                    Text = d.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListDownPaymentMethod(bool isNullItemFirst = false)
        {
            var result = CommonExtension.ToEnumSelectList<DownPaymentMethod>()
                                .Select(d => new SelectListItem
                                {
                                    Value = d.Value,
                                    Text = d.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListLeftPaymentMethod(bool isNullItemFirst = false)
        {
            var result = CommonExtension.ToEnumSelectList<LeftPaymentMethod>()
                                .Select(d => new SelectListItem
                                {
                                    Value = d.Value,
                                    Text = d.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListLeftPaymentPlace(bool isNullItemFirst = false)
        {
            var result = _context.MstPayments.Where(x => !x.IsDeleted)
                        .Select(o => new
                        {
                            o.Cd,
                            o.Name,
                        })
                        .AsEnumerable()
                        .Select(s => new SelectListItem
                        {
                            Value = s.Cd.ToString(),
                            Text = s.Name,
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListEvent(bool isNullItemFirst = false)
        {
            var result = _context.MstEvents.Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    x.Cd,
                    x.Code,
                    x.Name,
                    x.StartDate
                })
                .OrderByDescending(x => x.StartDate)
                .AsEnumerable()
                .Select(x => new SelectListItem()
                {
                    Value = x.Cd.ToString(),
                    Text = $"{x.Code} - {x.Name}",
                }).ToList();
            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem()
                {
                    Value = "",
                    Text = "催事選択"
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListSalesMen(bool isNullItemFirst = false)
        {
            var result = _context.MstSalesmen.Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    x.Cd,
                    x.Code,
                    x.Name
                })
                .OrderBy(x => x.Code)
                .AsEnumerable()
                .Select(x => new SelectListItem()
                {
                    Value = x.Cd.ToString(),
                    Text = string.Concat(x.Code, " - ", x.Name)
                }).ToList();
            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem()
                {
                    Value = "",
                    Text = ""
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListLeftPaymentMethods(bool isNullItemFirst = false)
        {
            var rs = CommonExtension.ToEnumSelectList<LeftPaymentMethod>()
                .Select(d => new SelectListItem
                {
                    Value = d.Value,
                    Text = d.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                rs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return rs;
        }

        public List<SelectListItem> GetSelectListClubs(bool isNullItemFirst = false)
        {
            var rs = CommonExtension.ToEnumSelectList<Club>()
                .Select(d => new SelectListItem
                {
                    Value = d.Value,
                    Text = d.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                rs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return rs;
        }

        public List<SelectListItem> GetSelectListNumberOfVisit(bool isNullItemFirst = false)
        {
            var rs = CommonExtension.ToEnumSelectList<NumberOfVisit>()
                .Select(d => new SelectListItem
                {
                    Value = d.Value,
                    Text = d.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                rs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return rs;
        }

        public List<SelectListItem> GetSelectListRemarks(bool isNullItemFirst = false)
        {
            var rs = CommonExtension.ToEnumSelectList<Remarks>()
                .Select(d => new SelectListItem
                {
                    Value = d.Value,
                    Text = d.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                rs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return rs;
        }

        public List<SelectListItem> GetSelectsListSalesman()
        {
            var result = _context.MstSalesmen.Where(s => !s.IsDeleted).Select(s => new
            {
                s.Cd,
                s.Code,
                s.Name,
                s.NameKana
            })
            .OrderBy(s => s.Code)
            .AsEnumerable()
            .Select(s => new SelectListItem
            {
                Value = s.Cd.ToString(),
                Text = string.Format("{0} - {1}", s.Code, s.Name),
            }).ToList();

            return result;
        }

        public List<ProductSelectListItem> GetSelectListProductByArtistCd(int artistCd, bool isNullItemFirst = false)
        {
            var result = _context.MstProducts.Where(x => !x.IsDeleted && x.ArtistCd == artistCd)
                .Select(p => new
                {
                    p.Cd,
                    p.Name,
                    p.Code
                })
                .OrderBy(p => p.Code)
                .AsEnumerable()
                .Select(p => new ProductSelectListItem()
                {
                    Cd = p.Cd.ToString(),
                    Name = p.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new ProductSelectListItem()
                {
                    Cd = "",
                    Name = "",
                    Price = "",
                });
            }

            return result;
        }

        public ProductSearchByCodeResult GetProductByCode(int? departmentCd, string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            var products = _context.MstProducts.Where(p => !p.IsDeleted && p.Artist.Code + p.Code + p.ItemCategory == code);

            if (departmentCd.HasValue)
            {
                products = products.Where(p => p.Artist.ArtistDepartments.Any(ad => ad.DepartmentCd == departmentCd));
            }

            var result = products
                        .Select(p => new
                        {
                            p.ArtistCd,
                            ArtistName = p.Artist.Name,
                            p.Cd,
                            p.OriginalName,
                            DepartmentCd = p.Artist.ArtistDepartments.Select(ad => ad.DepartmentCd).FirstOrDefault(),
                            TechniqueCd = p.ProductTechniques.Select(pt => pt.TechniqueCd).FirstOrDefault(),
                            TechniqueName = p.ProductTechniques.Select(pt => pt.Technique.Name).FirstOrDefault(),
                        })
                        .AsEnumerable()
                        .Select(p => new ProductSearchByCodeResult
                        {
                            ArtistCd = p.ArtistCd.ToString(),
                            ArtistName = p.ArtistName,
                            Cd = p.Cd.ToString(),
                            Name = p.OriginalName,
                            DepartmentCd = p.DepartmentCd > 0 ? p.DepartmentCd.ToString() : "",
                            TechniqueCd = p.TechniqueCd > 0 ? p.TechniqueCd.ToString() : "",
                            TechniqueName = p.TechniqueName
                        })
                        .FirstOrDefault();

            return result;
        }

        public List<SelectListItem> GetSelectListProvince(bool isNullItemFirst = false)
        {
            var result = _context.MstPrefectures
                        .OrderBy(p => p.Code)
                        .Select(p => new
                        {
                            p.Name,
                        })
                        .AsEnumerable()
                        .Select(s => new SelectListItem
                        {
                            Value = s.Name,
                            Text = s.Name,
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListGender(bool isNullItemFirst = false)
        {
            var genders = CommonExtension.ToEnumSelectList<Gender>()
                                .Select(g => new SelectListItem
                                {
                                    Value = g.Value,
                                    Text = g.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                genders.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return genders;
        }

        public List<SelectListItem> GetSelectListPhoneBranch(bool isNullItemFirst = false)
        {
            var branches = CommonExtension.ToEnumSelectList<PhoneBranch>()
                                .Select(b => new SelectListItem
                                {
                                    Value = b.Value,
                                    Text = b.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                branches.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return branches;
        }

        public List<SelectListItem> GetSelectListClubJoinStatus(bool isNullItemFirst = false)
        {
            var statuses = CommonExtension.ToEnumSelectList<StatusJoinClubMember>()
                                .Select(s => new SelectListItem
                                {
                                    Value = s.Value,
                                    Text = s.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                statuses.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return statuses;
        }

        public List<SelectListItem> GetSelectListIdentifyDoc(bool isNullItemFirst = false)
        {
            var docs = CommonExtension.ToEnumSelectList<IdentifyDocument>()
                                .Select(d => new SelectListItem
                                {
                                    Value = d.Value,
                                    Text = d.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                docs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return docs;
        }

        public List<SelectListItem> GetSelectListClubRegStatus(bool isNullItemFirst = false)
        {
            var statuses = CommonExtension.ToEnumSelectList<ClubJoin>()
                                .Select(s => new SelectListItem
                                {
                                    Value = s.Value,
                                    Text = s.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                statuses.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return statuses;
        }

        public List<SelectListItem> GetSelectListMemberCard(bool isNullItemFirst = false)
        {
            var cards = CommonExtension.ToEnumSelectList<MemberCard>()
                                .Select(c => new SelectListItem
                                {
                                    Value = c.Value,
                                    Text = c.Name,
                                }).ToList();

            if (isNullItemFirst)
            {
                cards.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return cards;
        }

        public string[] GetListAddress(string key)
        {
            var result = _context.MstAddresses
                        .Where(a => (a.Zip1 + a.Zip2).StartsWith(key))
                        .Select(a => new
                        {
                            a.Zip1,
                            a.Zip2,
                            a.Prefecture,
                            a.City,
                            a.Address,
                        })
                        .AsEnumerable()
                        .Select(a => $"{a.Zip1.Trim()}-{a.Zip2.Trim()} - {a.Prefecture} - {a.City}{(!string.IsNullOrEmpty(a.Address) ? $" - {a.Address}" : "")}")
                        .ToArray();

            return result;
        }

        public TechniqueSelectListItem[] GetListTechnique(string key, string productName)
        {
            var result = new TechniqueSelectListItem[0];

            if (string.IsNullOrEmpty(productName))
            {
                result = _context.MstTechniques
                        .Where(x => x.Name.ToUpper().StartsWith(key) && !x.IsDeleted)
                        .Select(x => new
                        {
                            x.Cd,
                            x.Name,
                        })
                        .AsEnumerable()
                        .Select(a => new TechniqueSelectListItem
                        {
                            Cd   = a.Cd.ToString(),
                            Name = a.Name
                        })
                        .ToArray();
            }
            else
            {
                result = _context.MstTechniques
                        .Where(x => x.Name.ToUpper().StartsWith(key) && x.ProductTechniques.Any(p=>p.Product.Name == productName) && !x.IsDeleted)
                        .Select(x => new
                        {
                            x.Cd,
                            x.Name,
                        })
                        .AsEnumerable()
                        .Select(a => new TechniqueSelectListItem
                        {
                            Cd   = a.Cd.ToString(),
                            Name = a.Name
                        })
                        .ToArray();
            }
            

            return result;
        }

        public AddressViewModel GetAddressByZipcode(string zip1, string zip2)
        {
            var result = _context.MstAddresses
                        .Where(a => a.Zip1.Trim() == zip1 && a.Zip2.Trim() == zip2)
                        .Select(a => new
                        {
                            a.Prefecture,
                            a.City,
                            a.Address,
                            a.PrefKana,
                            a.CityKana,
                            a.AddressKana,
                        })
                        .AsEnumerable()
                        .Select(a => new AddressViewModel
                        {
                            Province = a.Prefecture,
                            Address = $"{a.City}{(!string.IsNullOrEmpty(a.Address) ? $" {a.Address}" : "")}",
                            ProvinceKana = a.PrefKana,
                            AddressKana = $"{a.CityKana}{(!string.IsNullOrEmpty(a.AddressKana) ? $" {a.AddressKana}" : "")}",
                        })
                        .FirstOrDefault();

            return result;
        }

        public List<SelectListItem> GetEmptySelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "",
                }
            };
        }

        public List<SelectListItem> GetSelectListLeftPaymentPlace(int paymentMethod, int saleDepartment)
        {
            var category = 0;

            switch (paymentMethod)
            {
                case (int)LeftPaymentMethod.InCash:
                    category = (int)LeftPaymentPlaceCategory.InCash;
                    break;
                case (int)LeftPaymentMethod.BankTranfer:
                    category = (int)LeftPaymentPlaceCategory.BankTranfer;
                    break;
                case (int)LeftPaymentMethod.Card:
                    category = (int)LeftPaymentPlaceCategory.Card;
                    break;
                case (int)LeftPaymentMethod.Credit:
                    category = (int)LeftPaymentPlaceCategory.Credit;
                    break;
            }

            var payments = _context.MstPayments.Where(p => !p.IsDeleted && p.Category == category.ToString())
                        .Select(p => new
                        {
                            p.Cd,
                            p.Name,
                            p.Code,
                        })
                        .AsEnumerable()
                        .OrderBy(p => p.Code)
                        .Select(p => new SelectListItem
                        {
                            Value = p.Cd.ToString(),
                            Text = p.Name,
                        })
                        .ToList();
            
            var result = new List<SelectListItem>();

            if ((paymentMethod == (int)LeftPaymentMethod.Card || paymentMethod == (int)LeftPaymentMethod.Credit) && saleDepartment == 1)
            {
                result = payments
                        .Where(p => !p.Text.Contains(Constants.PaymentPlace_JN) && !p.Text.Contains(Constants.PaymentPlace_JN_Halfsize))
                        .OrderBy(p => p.Value)
                        .ToList();
            }


            if ((paymentMethod == (int)LeftPaymentMethod.Card || paymentMethod == (int)LeftPaymentMethod.Credit) && saleDepartment == 2)
            {
                result = payments
                        .Where(p => p.Text.Contains(Constants.PaymentPlace_JN) || p.Text.Contains(Constants.PaymentPlace_JN_Halfsize))
                        .OrderBy(p => p.Value)
                        .ToList();
            }

            result.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- 未選択 --",
            });

            return result;
        }

        public DateTime[] GetEventStartAndEndDate(string userId)
        {
            var eventDate = _context.MstEvents
                            .Where(e => !e.IsDeleted && e.ApplicationUserId == userId)
                            .Select(e => new
                            {
                                e.StartDate,
                                e.EndDate,
                            })
                            .FirstOrDefault();

            var result = new DateTime[2] { eventDate.StartDate ?? new DateTime(1, 1, 1), eventDate.EndDate ?? new DateTime(1, 1, 1) };

            return result;
        }

        public DateTimeEventModel GetDateTimeEvent(int eventCd)
        {
            var eventDate = _context.MstEvents
                            .Where(e => !e.IsDeleted && e.Cd == eventCd)
                            .Select(e => new
                            {
                                e.StartDate,
                                e.EndDate,
                            })
                            .FirstOrDefault();

            if(eventDate == null)
            {
                return null;
            }

            var result = new DateTimeEventModel
            {
                EventCd = eventCd,
                DateFrom = eventDate.StartDate?.ToString("yyyy-MM-dd"),
                DateTo = eventDate.EndDate?.ToString("yyyy-MM-dd"),
                DateFromFullStr = string.Format("{0}({1})", eventDate.StartDate?.ToString(ExactDateFormat), Utility.Commons.GetDayOfWeekJP(eventDate.StartDate)),
                DateToFullStr = string.Format("{0}({1})", eventDate.EndDate?.ToString(ExactDateFormat), Utility.Commons.GetDayOfWeekJP(eventDate.EndDate)),
            };

            return result;
        }

        public List<SelectListItem> GetSelectListSalesmanByEvent(int eventCd, bool isNullItemFirst = false)
        {
            var salesmen = _context.EventSalesAssigments
                //.Include(x => x.Salesman)
                .Where(x => x.EventCd == eventCd && !x.Salesman.IsDeleted)
                .Select(x => new
                {
                    x.Salesman.Cd,
                    x.Salesman.Code,
                    x.Salesman.Name
                })
                .AsEnumerable()
                .OrderBy(x => x.Code)
                .Select(x => new SelectListItem
                {
                    Value = x.Cd.ToString(),
                    Text = $"{x.Code} - {x.Name}",
                })
                .ToList();

            if (isNullItemFirst)
            {
                salesmen.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return salesmen;
        }

        public List<SelectListItem> GetSelectListIsHaveProductAgreement(bool isNullItemFirst = false)
        {
            var types = CommonExtension.ToEnumSelectList<IsHaveProductAgreement>()
                                .Select(h => new SelectListItem
                                {
                                    Value = h.Value,
                                    Text = h.Name,
                                })
                                .OrderByDescending(h => h.Value)
                                .ToList();

            if (isNullItemFirst)
            {
                types.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return types;
        }

        public List<SelectListItem> GetSelectListProductAgreementType(bool isNullItemFirst = false)
        {
            var result = _context.MstProductAgreementTypes.Where(x => !x.IsDeleted)
                        .Select(t => new
                        {
                            t.Cd,
                            t.Name,
                        })
                        .AsEnumerable()
                        .Select(t => new SelectListItem
                        {
                            Value = t.Cd.ToString(),
                            Text = t.Name,
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListVisitTime(bool isNullItemFirst = false)
        {
            var rs = CommonExtension.ToEnumSelectList<VisitTime>()
                .Select(t => new SelectListItem
                {
                    Value = t.Value,
                    Text = t.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                rs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return rs;
        }

        public List<SelectListItem> GetSelectListDepartment(bool isNullItemFirst = false)
        {
            var result = _context.MstDepartments.Where(d => !d.IsDeleted)
                        .Select(d => new
                        {
                            d.Cd,
                            d.Name,
                        })
                        .AsEnumerable()
                        .Select(d => new SelectListItem
                        {
                            Value = d.Cd.ToString(),
                            Text = d.Name,
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListDepartments(bool isNullItemFirst = false)
        {
            var result = _context.MstDepartments.Where(x => !x.IsDeleted)
                        .Select(d => new
                        {
                            d.Cd,
                            d.Name,
                            d.Code,
                        })
                        .OrderBy(d => d.Code)
                        .AsEnumerable()
                        .Select(d => new SelectListItem
                        {
                            Value = d.Cd.ToString(),
                            Text = d.Name,
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListEventMedia(string eventCode, bool isNullItemFirst = false)
        {
            var result = _context.EventMedias
                        .Where(em => em.Event.Code == eventCode && !em.Media.IsDeleted)
                        .Select(em => new
                        {
                            em.MediaCd,
                            em.Media.Name,
                            em.Media.Code,
                            em.Media.BranchCode,
                        })
                        .AsEnumerable()
                        .OrderBy(m => m.Code)
                        .Select(m => new SelectListItem
                        {
                            Value = m.MediaCd.ToString(),
                            Text = string.Format("{0}{1} - {2}", m.Code, m.BranchCode, m.Name),
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListEventMedias(bool isNullItemFirst = false)
        {
            var result = _context.MstMedias.Where(m => !m.IsDeleted).Select(m => new
            {
                m.Cd,
                m.Name,
                m.Code,
                m.BranchCode
            })
            .AsEnumerable()
            .OrderBy(m => m.Code)
            .Select(m => new SelectListItem
            {
                Text = string.Format("{0}{1} - {2}", m.Code, m.BranchCode, m.Name),
                Value = m.Cd.ToString(),
            }).ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListTechnique(bool isNullItemFirst = false)
        {
            var result = _context.MstTechniques.Where(t => !t.IsDeleted)
                        .Select(t => new
                        {
                            t.Cd,
                            t.Name,
                            t.Code,
                        })
                        .OrderBy(t => t.Code == NonTechniqueCode ? 0 : 1)
                        .AsEnumerable()
                        .Select(t => new SelectListItem
                        {
                            Value = t.Cd.ToString(),
                            Text = t.Name,
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListTechniqueByProductName(string productName = "", bool isNullItemFirst = false)
        {
            var techniques = _context.MstTechniques.Where(t => !t.IsDeleted);

            if (!string.IsNullOrEmpty(productName))
            {
                techniques = techniques.Where(t => t.ProductTechniques.Any(pt => pt.Product.OriginalName.Trim().ToLower() == productName.Trim()));
            }

            var result = techniques
                        .Select(t => new
                        {
                            t.Cd,
                            t.Name,
                            t.Code,
                        })
                        .OrderBy(t => t.Code == NonTechniqueCode ? 0 : 1)
                        .AsEnumerable()
                        .Select(t => new SelectListItem
                        {
                            Value = t.Cd.ToString(),
                            Text = t.Name,
                        })
                        .ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return result;
        }

        public List<EventSelectListItem> GetSelectListFutureEvent(string userId, DateTime currentEventStartDate, bool isNullItemFirst = false)
        {
            var result = _context.MstEvents.Where(e => !e.IsDeleted && e.StartDate >= currentEventStartDate && e.ApplicationUserId != userId)
                .Select(e => new
                {
                    e.Cd,
                    e.Name,
                    e.StartDate,
                    e.EndDate,
                })
                .OrderBy(e => e.StartDate)
                .AsEnumerable()
                .Select(e => new EventSelectListItem
                {
                    Value = e.Cd.ToString(),
                    Text = e.StartDate.HasValue && e.EndDate.HasValue
                            ? $"{e.Name} ー {e.StartDate?.ToString(ExactDateFormatJP)}～{e.EndDate?.ToString(ExactDateFormatJP)}"
                            : e.Name,
                    StartDate = e.StartDate?.ToString("yyyy-MM-dd"),
                    EndDate = e.EndDate?.ToString("yyyy-MM-dd"),
                }).ToList();

            if (isNullItemFirst)
            {
                result.Insert(0, new EventSelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                    StartDate = "",
                    EndDate = "",
                });
            }

            return result;
        }

        public List<SelectListItem> GetSelectListProductRemarkA(bool isNullItemFirst = false)
        {
            var rs = CommonExtension.ToEnumSelectList<ProductRemarkA>()
                .Select(d => new SelectListItem
                {
                    Value = d.Value,
                    Text = d.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                rs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return rs;
        }

        public List<SelectListItem> GetSelectListProductRemarkB(bool isNullItemFirst = false)
        {
            var rs = CommonExtension.ToEnumSelectList<ProductRemarkB>()
                .Select(d => new SelectListItem
                {
                    Value = d.Value,
                    Text = d.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                rs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return rs;
        }

        public List<SelectListItem> GetSelectListProductRemarkC(bool isNullItemFirst = false)
        {
            var rs = CommonExtension.ToEnumSelectList<ProductRemarkC>()
                .Select(d => new SelectListItem
                {
                    Value = d.Value,
                    Text = d.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                rs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return rs;
        }

        public List<SelectListItem> GetSelectListProductRemarkD(bool isNullItemFirst = false)
        {
            var rs = CommonExtension.ToEnumSelectList<ProductRemarkD>()
                .Select(d => new SelectListItem
                {
                    Value = d.Value,
                    Text = d.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                rs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return rs;
        }

        public List<SelectListItem> GetSelectListCashVoucherValue(bool isNullItemFirst = false)
        {
            var vouchers = CommonExtension.ToEnumSelectList<CashVoucherValue>()
                .Select(d => new SelectListItem
                {
                    Value = d.Value,
                    Text = d.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                vouchers.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- 未選択 --",
                });
            }

            return vouchers;
        }
    }
}
