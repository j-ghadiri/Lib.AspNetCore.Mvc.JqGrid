﻿using System;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Lib.AspNetCore.Mvc.JqGrid.Infrastructure.Enums;
using Lib.AspNetCore.Mvc.JqGrid.Infrastructure.Constants;
using Lib.AspNetCore.Mvc.JqGrid.Infrastructure.Options;
using Lib.AspNetCore.Mvc.JqGrid.Infrastructure.Options.ColumnModel;
using Lib.AspNetCore.Mvc.JqGrid.Helper.Constants;
using Lib.AspNetCore.Mvc.JqGrid.Helper.InternalHelpers;
using Lib.AspNetCore.Mvc.JqGrid.Infrastructure.Options.Navigator;

namespace Lib.AspNetCore.Mvc.JqGrid.Helper
{
    /// <summary>
    /// Provides support for generating jqGrid HMTL and JavaScript.
    /// </summary>
    public static class HtmlHelperJqGridExtensions
    {
        #region IHtmlHelper Extensions Methods
        /// <summary>
        /// Returns the HTML that is used to render the table placeholder for the grid. 
        /// </summary>
        /// <returns>The HTML that represents the table placeholder for jqGrid</returns>
        public static IHtmlContent JqGridTableHtml(this IHtmlHelper htmlHelper, JqGridOptions options)
        {
            return new HtmlString(String.Format("<table id='{0}'></table>", options.Id));
        }

        /// <summary>
        /// Returns the HTML that is used to render the table placeholder for the grid with pager placeholder below it and filter grid (if enabled) placeholder above it.
        /// </summary>
        /// <returns>The HTML that represents the table placeholder for jqGrid with pager placeholder below i</returns>
        public static IHtmlContent JqGridHtml(this IHtmlHelper htmlHelper, JqGridOptions options)
        {
            return htmlHelper.JqGridTableHtml(options);
        }

        /// <summary>
        /// Return the JavaScript that is used to initialize jqGrid with given options.
        /// </summary>
        /// <returns>The JavaScript that initializes jqGrid with give options</returns>
        /// <exception cref="System.InvalidOperationException">
        /// <list type="bullet">
        /// <item><description>TreeGrid and data grouping are both enabled.</description></item>
        /// <item><description>Rows numbers and data grouping are both enabled.</description></item>
        /// <item><description>Dynamic scrolling and data grouping are both enabled.</description></item>
        /// <item><description>TreeGrid and GridView are both enabled.</description></item>
        /// <item><description>SubGrid and GridView are both enabled.</description></item>
        /// <item><description>AfterInsertRow event and GridView are both enabled.</description></item>
        /// </list> 
        /// </exception>
        public static IHtmlContent JqGridJavaScript(this IHtmlHelper htmlHelper, JqGridOptions options)
        {
            ValidateJqGridConstraints(options);

            options.ApplyModelMetadata(htmlHelper.MetadataProvider);

            StringBuilder javaScriptBuilder = new StringBuilder();

            javaScriptBuilder.AppendFormat("$({0}).jqGrid({{", GetJqGridGridSelector(options, false)).AppendLine()
                .AppendColumnsNames(options)
                .AppendColumnsModels(options)
                .Append("})");

            javaScriptBuilder.AppendLine(";");

            return new HtmlString(javaScriptBuilder.ToString());
        }
        #endregion

        #region Private Methods
        private static void ValidateJqGridConstraints(JqGridOptions options)
        { }

        private static string GetJqGridGridSelector(JqGridOptions options, bool asSubgrid)
        {
            return asSubgrid ? "'#' + subgridTableId" : String.Format("'#{0}'", options.Id);
        }

        private static StringBuilder AppendColumnsNames(this StringBuilder javaScriptBuilder, JqGridOptions options)
        {
            javaScriptBuilder.AppendJavaScriptArrayFieldOpening(JqGridOptionsNames.COLUMNS_NAMES_FIELD);

            foreach (string columnName in options.ColumnsNames)
            {
                javaScriptBuilder.AppendJavaScriptArrayStringValue(columnName);
            }

            javaScriptBuilder.AppendJavaScriptArrayFieldClosing()
                .AppendLine();

            return javaScriptBuilder;
        }

        private static StringBuilder AppendColumnsModels(this StringBuilder javaScriptBuilder, JqGridOptions options)
        {
            javaScriptBuilder.AppendJavaScriptArrayFieldOpening(JqGridOptionsNames.COLUMNS_MODEL_FIELD);

            foreach(JqGridColumnModel columnModel in options.ColumnsModels)
            {
                javaScriptBuilder.AppendJavaScriptObjectOpening()
                    .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.NAME_FIELD, columnModel.Name)
                    .AppendColumnModelSortOptions(columnModel)
                    .AppendColumnModelFormatter(columnModel);

                javaScriptBuilder.AppendJavaScriptObjectFieldClosing();
            }

            javaScriptBuilder.AppendJavaScriptArrayFieldClosing()
                .AppendLine();

            return javaScriptBuilder;
        }

        private static StringBuilder AppendColumnModelSortOptions(this StringBuilder javaScriptBuilder, JqGridColumnModel columnModel)
        {
            javaScriptBuilder.AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.INDEX_FIELD, columnModel.Index)
                .AppendJavaScriptObjectEnumField(JqGridOptionsNames.ColumnModel.INITIAL_SORTING_ORDER_FIELD, columnModel.InitialSortingOrder, JqGridOptionsDefaults.ColumnModel.Sorting.InitialOrder);

            if (columnModel.Sortable != JqGridOptionsDefaults.ColumnModel.Sorting.Sortable)
            {
                javaScriptBuilder.AppendJavaScriptObjectBooleanField(JqGridOptionsNames.ColumnModel.SORTABLE_FIELD, columnModel.Sortable);
            }
            else
            {
                if (columnModel.SortType == JqGridSortTypes.Function)
                {
                    javaScriptBuilder.AppendJavaScriptObjectFunctionField(JqGridOptionsNames.ColumnModel.SORT_TYPE_FIELD, columnModel.SortFunction);
                }
                else
                {
                    javaScriptBuilder.AppendJavaScriptObjectEnumField(JqGridOptionsNames.ColumnModel.SORT_TYPE_FIELD, columnModel.SortType, JqGridOptionsDefaults.ColumnModel.Sorting.Type);
                }
            }

            return javaScriptBuilder;
        }

        private static StringBuilder AppendColumnModelFormatter(this StringBuilder javaScriptBuilder, JqGridColumnModel columnModel)
        {
            if (!String.IsNullOrWhiteSpace(columnModel.Formatter))
            {
                if (columnModel.Formatter == JqGridPredefinedFormatters.JQueryUIButton)
                {
                    javaScriptBuilder.AppendColumnModelJQueryUIButtonFormatter(columnModel.FormatterOptions);
                }
                else
                {
                    javaScriptBuilder.AppendJavaScriptObjectFunctionField(JqGridOptionsNames.ColumnModel.FORMATTER_FIELD, columnModel.Formatter)
                        .AppendColumnModelFormatterOptions(columnModel.Formatter, columnModel.FormatterOptions)
                        .AppendJavaScriptObjectFunctionField(JqGridOptionsNames.ColumnModel.UNFORMATTER_FIELD, columnModel.UnFormatter);
                }
            }

            return javaScriptBuilder;
        }

        private static StringBuilder AppendColumnModelJQueryUIButtonFormatter(this StringBuilder javaScriptBuilder, JqGridColumnFormatterOptions formatterOptions)
        {

            return javaScriptBuilder;
        }

        private static StringBuilder AppendColumnModelFormatterOptions(this StringBuilder javaScriptBuilder, string formatter, JqGridColumnFormatterOptions  formatterOptions)
        {
            if ((formatterOptions) != null && !formatterOptions.AreDefault(formatter))
            {
                javaScriptBuilder.AppendJavaScriptObjectFieldOpening(JqGridOptionsNames.ColumnModel.FORMATTER_OPTIONS_FIELD);

                switch (formatter)
                {
                    case JqGridPredefinedFormatters.Integer:
                        javaScriptBuilder.AppendColumnModelIntegerFormatterOptions(formatterOptions);
                        break;
                    case JqGridPredefinedFormatters.Number:
                        javaScriptBuilder.AppendColumnModelNumberFormatterOptions(formatterOptions);
                        break;
                    case JqGridPredefinedFormatters.Currency:
                        javaScriptBuilder.AppendColumnModelCurrencyFormatterOptions(formatterOptions);
                        break;
                    case JqGridPredefinedFormatters.Date:
                        javaScriptBuilder.AppendColumnModelDateFormatterOptions(formatterOptions);
                        break;
                    case JqGridPredefinedFormatters.Link:
                        javaScriptBuilder.AppendColumnModelLinkFormatterOptions(formatterOptions);
                        break;
                    case JqGridPredefinedFormatters.ShowLink:
                        javaScriptBuilder.AppendColumnModelShowLinkFormatterOptions(formatterOptions);
                        break;
                    case JqGridPredefinedFormatters.CheckBox:
                        javaScriptBuilder.AppendColumnModelCheckBoxFormatterOptions(formatterOptions);
                        break;
                    case JqGridPredefinedFormatters.Actions:
                        javaScriptBuilder.AppendColumnModelActionsFormatterOptions(formatterOptions);
                        break;
                }

                javaScriptBuilder.AppendJavaScriptObjectFieldClosing();
            }

            return javaScriptBuilder;
        }

        private static StringBuilder AppendColumnModelIntegerFormatterOptions(this StringBuilder javaScriptBuilder, JqGridColumnFormatterOptions formatterOptions)
        {
            return javaScriptBuilder.AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.DEFAULT_VALUE, formatterOptions.DefaultValue, JqGridOptionsDefaults.ColumnModel.Formatter.IntegerDefaultValue)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.THOUSANDS_SEPARATOR, formatterOptions.ThousandsSeparator, JqGridOptionsDefaults.ColumnModel.Formatter.ThousandsSeparator);
        }

        private static StringBuilder AppendColumnModelNumberFormatterOptions(this StringBuilder javaScriptBuilder, JqGridColumnFormatterOptions formatterOptions)
        {
            return javaScriptBuilder.AppendJavaScriptObjectIntegerField(JqGridOptionsNames.ColumnModel.Formatter.DECIMAL_PLACES, formatterOptions.DecimalPlaces, JqGridOptionsDefaults.ColumnModel.Formatter.DecimalPlaces)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.DECIMAL_SEPARATOR, formatterOptions.DecimalSeparator, JqGridOptionsDefaults.ColumnModel.Formatter.DecimalSeparator)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.DEFAULT_VALUE, formatterOptions.DefaultValue, JqGridOptionsDefaults.ColumnModel.Formatter.NumberDefaultValue)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.THOUSANDS_SEPARATOR, formatterOptions.ThousandsSeparator, JqGridOptionsDefaults.ColumnModel.Formatter.ThousandsSeparator);
        }

        private static StringBuilder AppendColumnModelCurrencyFormatterOptions(this StringBuilder javaScriptBuilder, JqGridColumnFormatterOptions formatterOptions)
        {
            return javaScriptBuilder.AppendJavaScriptObjectIntegerField(JqGridOptionsNames.ColumnModel.Formatter.DECIMAL_PLACES, formatterOptions.DecimalPlaces, JqGridOptionsDefaults.ColumnModel.Formatter.DecimalPlaces)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.DECIMAL_SEPARATOR, formatterOptions.DecimalSeparator, JqGridOptionsDefaults.ColumnModel.Formatter.DecimalSeparator)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.DEFAULT_VALUE, formatterOptions.DefaultValue, JqGridOptionsDefaults.ColumnModel.Formatter.NumberDefaultValue)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.PREFIX, formatterOptions.Prefix)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.SUFFIX, formatterOptions.Suffix)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.THOUSANDS_SEPARATOR, formatterOptions.ThousandsSeparator, JqGridOptionsDefaults.ColumnModel.Formatter.ThousandsSeparator);
        }

        private static StringBuilder AppendColumnModelDateFormatterOptions(this StringBuilder javaScriptBuilder, JqGridColumnFormatterOptions formatterOptions)
        {
            return javaScriptBuilder.AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.SOURCE_FORMAT, formatterOptions.SourceFormat, JqGridOptionsDefaults.ColumnModel.Formatter.SourceFormat)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.OUTPUT_FORMAT, formatterOptions.OutputFormat, JqGridOptionsDefaults.ColumnModel.Formatter.OutputFormat);
        }

        private static StringBuilder AppendColumnModelLinkFormatterOptions(this StringBuilder javaScriptBuilder, JqGridColumnFormatterOptions formatterOptions)
        {
            return javaScriptBuilder.AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.TARGET, formatterOptions.Target);
        }

        private static StringBuilder AppendColumnModelShowLinkFormatterOptions(this StringBuilder javaScriptBuilder, JqGridColumnFormatterOptions formatterOptions)
        {
            return javaScriptBuilder.AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.BASE_LINK_URL, formatterOptions.BaseLinkUrl)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.SHOW_ACTION, formatterOptions.ShowAction)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.ADD_PARAM, formatterOptions.AddParam)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.TARGET, formatterOptions.Target)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.ColumnModel.Formatter.ID_NAME, formatterOptions.IdName, JqGridOptionsDefaults.ColumnModel.Formatter.IdName);
        }

        private static StringBuilder AppendColumnModelCheckBoxFormatterOptions(this StringBuilder javaScriptBuilder, JqGridColumnFormatterOptions formatterOptions)
        {
            return javaScriptBuilder.AppendJavaScriptObjectBooleanField(JqGridOptionsNames.ColumnModel.Formatter.DISABLED, formatterOptions.Disabled);
        }

        private static StringBuilder AppendColumnModelActionsFormatterOptions(this StringBuilder javaScriptBuilder, JqGridColumnFormatterOptions formatterOptions)
        {
            javaScriptBuilder.AppendJavaScriptObjectBooleanField(JqGridOptionsNames.ColumnModel.Formatter.EDIT_BUTTON, formatterOptions.EditButton, JqGridOptionsDefaults.ColumnModel.Formatter.EditButton)
                .AppendJavaScriptObjectBooleanField(JqGridOptionsNames.ColumnModel.Formatter.DELETE_BUTTON, formatterOptions.DeleteButton, JqGridOptionsDefaults.ColumnModel.Formatter.DeleteButton)
                .AppendJavaScriptObjectBooleanField(JqGridOptionsNames.ColumnModel.Formatter.USE_FORM_EDITING, formatterOptions.UseFormEditing, JqGridOptionsDefaults.ColumnModel.Formatter.UseFormEditing);

            if (formatterOptions.EditButton)
            {
                if (!formatterOptions.UseFormEditing && (formatterOptions.InlineEditingOptions != null) && formatterOptions.InlineEditingOptions.AreDefault())
                {
                    javaScriptBuilder.AppendInlineNavigatorActionOptions(formatterOptions.InlineEditingOptions);
                }
                else if (formatterOptions.UseFormEditing && (formatterOptions.FormEditingOptions != null) && formatterOptions.FormEditingOptions.AreDefault())
                {
                    //javaScriptBuilder.AppendNavigatorActionOptions("editOptions: ", formatterOptions.FormEditingOptions);
                }
            }

            if (formatterOptions.DeleteButton && (formatterOptions.DeleteOptions != null) && formatterOptions.DeleteOptions.AreDefault())
            {
                //javaScriptBuilder.AppendNavigatorActionOptions("delOptions: ", formatterOptions.DeleteOptions);
            }


            return javaScriptBuilder;
        }

        private static StringBuilder AppendInlineNavigatorActionOptions(this StringBuilder javaScriptBuilder, JqGridInlineNavigatorActionOptions inlineNavigatorActionOptions)
        {
            if (!String.IsNullOrWhiteSpace(inlineNavigatorActionOptions.ExtraParamScript))
            {
                javaScriptBuilder.AppendJavaScriptObjectFunctionField(JqGridOptionsNames.InlineNavigator.EXTRA_PARAM, inlineNavigatorActionOptions.ExtraParamScript);
            }
            else if (inlineNavigatorActionOptions.ExtraParam != null)
            {
                javaScriptBuilder.AppendJavaScriptObjectFunctionField(JqGridOptionsNames.InlineNavigator.EXTRA_PARAM, JsonConvert.SerializeObject(inlineNavigatorActionOptions.ExtraParam, Formatting.None));
            }

            return javaScriptBuilder.AppendJavaScriptObjectBooleanField(JqGridOptionsNames.InlineNavigator.KEYS, inlineNavigatorActionOptions.Keys, JqGridOptionsDefaults.Navigator.InlineActionKeys)
                .AppendJavaScriptObjectFunctionField(JqGridOptionsNames.InlineNavigator.ON_EDIT_FUNCTION, inlineNavigatorActionOptions.OnEditFunction)
                .AppendJavaScriptObjectFunctionField(JqGridOptionsNames.InlineNavigator.SUCCESS_FUNCTION, inlineNavigatorActionOptions.SuccessFunction)
                .AppendJavaScriptObjectStringField(JqGridOptionsNames.InlineNavigator.URL, inlineNavigatorActionOptions.Url)
                .AppendJavaScriptObjectFunctionField(JqGridOptionsNames.InlineNavigator.AFTER_SAVE_FUNCTION, inlineNavigatorActionOptions.AfterSaveFunction)
                .AppendJavaScriptObjectFunctionField(JqGridOptionsNames.InlineNavigator.ERROR_FUNCTION, inlineNavigatorActionOptions.ErrorFunction)
                .AppendJavaScriptObjectFunctionField(JqGridOptionsNames.InlineNavigator.AFTER_RESTORE_FUNCTION, inlineNavigatorActionOptions.AfterRestoreFunction)
                .AppendJavaScriptObjectBooleanField(JqGridOptionsNames.InlineNavigator.RESTORE_AFTER_ERROR, inlineNavigatorActionOptions.RestoreAfterError, JqGridOptionsDefaults.Navigator.InlineActionRestoreAfterError)
                .AppendJavaScriptObjectEnumField(JqGridOptionsNames.InlineNavigator.METHOD_TYPE, inlineNavigatorActionOptions.MethodType, JqGridOptionsDefaults.Navigator.InlineActionMethodType);
        }
        #endregion
    }
}
