﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Domain;
using Google.Apis.Sheets.v4.Data;

namespace GoogleSheetWriter
{
    internal class GoogleDataWrapper : IExpense
    {
        private readonly string[] _cellData;
        private readonly IndicesMapping _indices;
        private readonly CultureInfo _culture;

        internal GoogleDataWrapper(IList<CellData> cellData, IndicesMapping indices, CultureInfo culture)
        {
            _cellData = cellData.Select(c => c.FormattedValue).ToArray();
            _indices = indices;
            _culture = culture;
        }

        public DateOnly Date => DateOnly.Parse(_cellData[_indices.DateIndex], _culture);

        public string Category =>
            _indices.DefaultCategory != null ? _indices.DefaultCategory! :
            _indices.CategoryIndex != null ? _cellData[_indices.CategoryIndex.Value] : "UNKNOWN";

        public string? SubCategory => GetByIndex(_indices.SubcategoryIndex);
        public string? Description => GetByIndex(_indices.DescriptionIndex);

        public Money Amount => ParseMoney(GetByIndex(_indices.RurAmountIndex), GetByIndex(_indices.AmdAmountIndex), GetByIndex(_indices.GelAmountIndex));

        private string? GetByIndex(int? index)
        {
            if (index == null || _cellData.Length <= index.Value) return null;

            return _cellData[index.Value];
        }

        private Money ParseMoney(string? rurValue, string? amdValue, string? gelValue)
        {
            string rur = (rurValue ?? String.Empty).Trim();
            string amd = (amdValue ?? String.Empty).Trim();
            string gel = (gelValue ?? String.Empty).Trim();
            if (string.IsNullOrEmpty(rur) && string.IsNullOrEmpty(amd) && string.IsNullOrEmpty(gel))
                return new Money() {Currency = Currency.Amd, Amount = 0m};

            if (rur.Contains("Загрузка", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Money() {Currency = Currency.Rur, Amount = 0m};
            }

            if (Money.TryParse(rur, Currency.Rur, _culture, out var money))
            {
                return money;
            }

            else if (Money.TryParse(amd, Currency.Amd, _culture, out money))
            {
                return money;
            }
            
            else if (Money.TryParse(gel, Currency.Gel, _culture, out money))
            {
                return money;
            }

            throw new ArgumentOutOfRangeException($"Couldn't parse money from {rurValue}, {amdValue}, and {gelValue}");
        }
    }
}