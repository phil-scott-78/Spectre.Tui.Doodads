using Shouldly;
using Spectre.Tui.Doodads.Doodads.Paginator;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class PaginatorModelTests
{
    [Fact]
    public void SetTotalPages_Should_Calculate_From_Items()
    {
        var p = new PaginatorModel { PerPage = 5 }.SetTotalPages(23);

        p.TotalPages.ShouldBe(5); // ceil(23/5) = 5
    }

    [Fact]
    public void SetTotalPages_Should_Clamp_Page()
    {
        var p = new PaginatorModel { PerPage = 5, Page = 99 }.SetTotalPages(10);

        p.Page.ShouldBe(1); // max page is 1 (2 pages: 0,1)
    }

    [Fact]
    public void GetSliceBounds_Should_Return_Correct_Range()
    {
        var p = new PaginatorModel { PerPage = 5, Page = 1 }.SetTotalPages(23);

        var (start, end) = p.GetSliceBounds(23);
        start.ShouldBe(5);
        end.ShouldBe(10);
    }

    [Fact]
    public void GetSliceBounds_On_Last_Page_Should_Clamp()
    {
        var p = new PaginatorModel { PerPage = 5, Page = 4 }.SetTotalPages(23);

        var (start, end) = p.GetSliceBounds(23);
        start.ShouldBe(20);
        end.ShouldBe(23);
    }

    [Fact]
    public void ItemsOnPage_Should_Return_Count()
    {
        var p = new PaginatorModel { PerPage = 5, Page = 4 }.SetTotalPages(23);

        p.ItemsOnPage(23).ShouldBe(3);
    }

    [Fact]
    public void NextPage_Should_Advance()
    {
        var p = new PaginatorModel { PerPage = 5 }.SetTotalPages(20);
        var (next, _) = p.NextPage();

        next.Page.ShouldBe(1);
    }

    [Fact]
    public void NextPage_On_Last_Page_Should_Not_Change()
    {
        var p = new PaginatorModel { PerPage = 5, Page = 3 }.SetTotalPages(20);
        var (next, _) = p.NextPage();

        next.Page.ShouldBe(3);
    }

    [Fact]
    public void PrevPage_Should_Go_Back()
    {
        var p = (new PaginatorModel { PerPage = 5 }.SetTotalPages(20)) with { Page = 2 };
        var (prev, _) = p.PrevPage();

        prev.Page.ShouldBe(1);
    }

    [Fact]
    public void PrevPage_On_First_Page_Should_Not_Change()
    {
        var p = new PaginatorModel { PerPage = 5 }.SetTotalPages(20);
        var (prev, _) = p.PrevPage();

        prev.Page.ShouldBe(0);
    }

    [Fact]
    public void OnFirstPage_Should_Be_True_Initially()
    {
        var p = new PaginatorModel();
        p.OnFirstPage.ShouldBeTrue();
    }

    [Fact]
    public void OnLastPage_With_Single_Page()
    {
        var p = new PaginatorModel { TotalPages = 1 };
        p.OnLastPage.ShouldBeTrue();
    }
}