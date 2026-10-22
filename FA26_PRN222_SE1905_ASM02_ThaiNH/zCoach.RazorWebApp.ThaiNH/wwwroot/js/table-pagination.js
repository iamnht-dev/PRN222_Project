/*
 * table-pagination.js
 * Phân trang phía client bằng jQuery cho bảng HTML.
 * Dùng chung cho ASM1 (MVC), ASM2 (Razor Pages) và ASM3 (Blazor).
 */
(function (window, $) {
    "use strict";

    if (!$) {
        return;
    }

    function toInt(value, fallback) {
        var parsed = parseInt(value, 10);
        return isNaN(parsed) || parsed <= 0 ? fallback : parsed;
    }

    function buildPageList(current, total) {
        var pages = [];
        var i;

        if (total <= 7) {
            for (i = 1; i <= total; i++) {
                pages.push(i);
            }
            return pages;
        }

        pages.push(1);

        if (current > 3) {
            pages.push("...");
        }

        var start = Math.max(2, current - 1);
        var end = Math.min(total - 1, current + 1);

        for (i = start; i <= end; i++) {
            pages.push(i);
        }

        if (current < total - 2) {
            pages.push("...");
        }

        pages.push(total);
        return pages;
    }

    function init(options) {
        var settings = $.extend({
            table: null,
            pager: null,
            info: null,
            pageSizeSelector: null,
            pageSize: 10,
            labels: {
                previous: "&laquo;",
                next: "&raquo;",
                empty: "Không có dữ liệu",
                showing: "Hiển thị {0}-{1} / {2} bản ghi"
            }
        }, options);

        var $table = $(settings.table);
        if (!$table.length) {
            return;
        }

        var $tbody = $table.find("tbody").first();
        var $pager = $(settings.pager);
        var $info = $(settings.info);
        var $pageSize = $(settings.pageSizeSelector);

        var $allRows = $tbody.children("tr").filter(function () {
            return !$(this).hasClass("js-empty-row");
        });

        var pageSize = toInt($pageSize.val(), settings.pageSize);
        var currentPage = 1;

        function render() {
            var totalRows = $allRows.length;
            var pageCount = Math.max(1, Math.ceil(totalRows / pageSize));

            if (currentPage > pageCount) {
                currentPage = pageCount;
            }

            var startIndex = (currentPage - 1) * pageSize;
            var endIndex = Math.min(startIndex + pageSize, totalRows);

            $allRows.hide().slice(startIndex, endIndex).show();

            if (totalRows === 0) {
                $pager.empty();
                $info.text(settings.labels.empty);
                return;
            }

            $info.text(settings.labels.showing
                .replace("{0}", startIndex + 1)
                .replace("{1}", endIndex)
                .replace("{2}", totalRows));

            var $ul = $('<ul class="pagination pagination-sm mb-0"></ul>');

            var $prev = $('<li class="page-item"></li>');
            if (currentPage === 1) {
                $prev.addClass("disabled");
                $prev.append('<span class="page-link">' + settings.labels.previous + "</span>");
            } else {
                $prev.append('<a class="page-link" href="#" data-page="' + (currentPage - 1) + '">' + settings.labels.previous + "</a>");
            }
            $ul.append($prev);

            $.each(buildPageList(currentPage, pageCount), function (_, page) {
                if (page === "...") {
                    $ul.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
                    return;
                }

                var $li = $('<li class="page-item"></li>');
                if (page === currentPage) {
                    $li.addClass("active");
                    $li.append('<span class="page-link">' + page + "</span>");
                } else {
                    $li.append('<a class="page-link" href="#" data-page="' + page + '">' + page + "</a>");
                }
                $ul.append($li);
            });

            var $next = $('<li class="page-item"></li>');
            if (currentPage === pageCount) {
                $next.addClass("disabled");
                $next.append('<span class="page-link">' + settings.labels.next + "</span>");
            } else {
                $next.append('<a class="page-link" href="#" data-page="' + (currentPage + 1) + '">' + settings.labels.next + "</a>");
            }
            $ul.append($next);

            $pager.empty().append($ul);
        }

        $pager.off("click.tp").on("click.tp", "a[data-page]", function (event) {
            event.preventDefault();
            currentPage = toInt($(this).data("page"), 1);
            render();
        });

        $pageSize.off("change.tp").on("change.tp", function () {
            pageSize = toInt($(this).val(), settings.pageSize);
            currentPage = 1;
            render();
        });

        render();
    }

    window.tablePagination = { init: init };
})(window, window.jQuery);
