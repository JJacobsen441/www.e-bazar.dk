/*
 * Common
 * */

function format(str) {
    var a = "&nbsp;";
    //var a = " ";
    var b = "æ";
    var c = "ø";
    var d = "å";

    str = str.replace(/\s/g, "");
    while (str.includes(a))
        str = str.replace(a, "");
    while (str.includes(b))
        str = str.replace(b, "ae");
    while (str.includes(c))
        str = str.replace(c, "oe");
    while (str.includes(d))
        str = str.replace(d, "aa");
    
    return str.toLowerCase();
}

function isEmptyObject(obj) {
    var name;
    for (name in obj) {
        return false;
    }
    return true;
}

function myDecode(str) {
    //alert('ok');
    return str.replace('&#230;', 'æ').replace('&#248;', 'ø').replace('&#229;', 'å').replace('&#198;', 'Æ').replace('&#216;', 'Ø').replace('&#197;', 'Å');
}

function removeFromString(cats, cat, split) {
    var res = '';
    var arr = format(cats).split(split);
    for (var i = 0; i < arr.length; i++) {

        var c = arr[i];
        if (c != format(cat) && arr[i] != '') {
            res += c + split;
        }
    }
    return res;
}

function myalert(msg) {
    //alert('hep ' + msg);
    fade = false;
    $('.z-loading').fadeIn(400);
    $('.z-myalert').removeClass('z-display-none');
    $('.z-myalert').fadeIn(400);
    $('.z-myalert .z-myalert-txt').html(msg);
    //alert('yes');
}

function showErrors() {
    var j = $('#err_msg').val();
    //alert(j);
    if (typeof (j) === 'undefined' || j == '')
        return;
    var json = JSON.parse(j);
    var arr = [json];
    var error_str = '';

    if (!isEmptyObject(json)) {//if (json == '[object Object]') {
    //alert('hep2');
        for (var i = 0; i < arr.length; i++) {
            var obj = arr[i];
            for (var key in obj) {
                var attrName = key;
                var attrValue = obj[key];
                if (attrValue != '')
                    error_str += attrValue + '<br />';
            }
        }
        //alert('hepy ' + error_str);
        myalert(error_str);
        //alert('ok');
        return true;
    }
    else
        $('.z-myalert').hide();
    return false;

}

function showMessages() {
    var j = $('#sys_msg').val();
    //alert(j);
    if (typeof (j) === 'undefined' || j == '')
        return;
    var json = JSON.parse($('#sys_msg').val());

    var arr = [json];
    var error_str = '';
    if (!isEmptyObject(json)) {//if (json == '[object Object]') {
    //alert('hep1');
        for (var i = 0; i < arr.length; i++) {
            var obj = arr[i];
            for (var key in obj) {
                var attrName = key;
                var attrValue = obj[key];
                if (attrValue != '')
                    error_str += attrValue + '\n';
            }
        }
        myalert(error_str);
    }
}





 /* 
 * Run All
 * */

window.onpageshow = function (event) {//gør måske ikke noget, men lader at at få javascript til at køre i safari
    if (event.persisted) {
        $('.z-loading-page').hide();
    }
};

var afterlater = 0;
window.onunload = function () { };
//$('.loading_page').fadeOut(400);
function afterlater_func() {
    var downloadingImages = [];
    var counter = 0;
    $(".afterlater").each(function (index, element) {
        downloadingImages[counter] = new Image();
        downloadingImages[counter].onload = function () {
            $(element).attr("src", this.src);
        };
        downloadingImages[counter].src = $(element).data("src");
        counter++;
    });
}

$(window).on("load", function () {
    var downloadingImages = [];
    var counter = 0;
    $(".loadlater").each(function (index, element) {
        downloadingImages[counter] = new Image();
        downloadingImages[counter].onload = function () {
            $(element).attr("src", this.src);
            afterlater++;
            if (afterlater == 6)
                afterlater_func();
        };
        downloadingImages[counter].src = $(element).data("src");
        counter++;
    });
});











/*
 * Layout 
 * */

var fade = true;
var fade_running = false;
var width_old = 0;
$(document).ready(function () {
    //alert('1');
    fix_mobile();
    just_remove();
    if (fade)
        $('.z-loading-page').fadeOut(400);

    if ('True' == $('#cook').val()) {
        //alert('hep');
        $('.z-cookie').hide();
    }

    $(".myfade").click(function () {
        $('.z-loading').fadeIn(200);
    });
    $(".myfade").change(function () {
        $('.z-loading').fadeIn(200);
    });
    $('.z-cookie-btn').click(function () {
        var path = "/Marketplace/Cookie";
        cookie_Ajax(path);
    });
    var navbar_show = false;
    $('.z-navbar').hide();
    $('button.z-navbar-toggle').click(function () {
        navbar_show = !navbar_show;
        if (navbar_show)
            $('.z-navbar').slideDown();
        else
            $('.z-navbar').slideUp();
    });

    var fav_show = false;
    $('.z-fav').hide();
    $('button.z-fav-toggle').click(function () {
        fav_show = !fav_show;
        if (fav_show)
            $('.z-fav').show();
        else
            $('.z-fav').hide();
    });

    var fol_show = false;
    $('.z-fol').hide();
    $('button.z-fol-toggle').click(function () {
        fol_show = !fol_show;
        if (fol_show)
            $('.z-fol').show();
        else
            $('.z-fol').hide();
    });
    $('.z-mp-tooltip2').click(function () {
        var span = $(this).find('.z-mp-content-tooltip');

        $(span).css('display', 'inline');
        $(span).css('position', 'absolute');
        $(span).css('padding', '15px');

        $(span).css('color', '#FFF');
        $(span).css('background-color', '#212121');
        $(span).css('border', '2px solid black');
        $(span).css('border-radius', '10px');
        $(span).css('opacity', '0.9');
    });

    $('.tooltip2-close').click(function (e) {
        var span = $(this).closest('.z-mp-content-tooltip');
        $(span).css('display', 'none');
        return false;
    });

    $('.z-mp-tooltip3').click(function () {
        var span = $(this).find('.z-mp-content-tooltip');

        $(span).css('display', 'inline');
        $(span).css('position', 'absolute');
        $(span).css('padding', '15px');

        $(span).css('color', '#FFF');
        $(span).css('background-color', '#212121');
        $(span).css('border', '2px solid black');
        $(span).css('border-radius', '10px');
        $(span).css('opacity', '0.9');
    });

    $('.tooltip3-close').click(function (e) {
        var span = $(this).closest('.z-mp-content-tooltip');
        $(span).css('display', 'none');
        return false;
    });

    $('.z-tooltip7').click(function () {
        var span = $(this).find('.z-desc-tooltip7');

        $(span).css('display', 'inline');
        $(span).css('position', 'absolute');
        $(span).css('padding', '15px');

        $(span).css('color', '#FFF');
        $(span).css('background-color', '#212121');
        $(span).css('border', '2px solid black');
        $(span).css('border-radius', '10px');
        $(span).css('opacity', '0.9');
    });

    $('.tooltip7-close').click(function (e) {
        var span = $(this).closest('.z-desc-tooltip7');
        $(span).css('display', 'none');
        return false;
    });
    //alert('yes');
    $(window).on('resize', function () {
        fix_mobile();
    });
    //alert('no');
    //alert('ready');
    ready_arc();
    //alert('ready2');
    ready_nav();
    //alert('ready3');
    ready_booth();
    //alert('done');

    setInterval(myRefresh, 3000);
});

function cookie_Ajax(path) {
    //alert('ok');
    $.ajax({
        url: path,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        //data: JSON.stringify(data),
        success: function (results) {
            if (results.success) {
                $('.z-cookie').hide();
            }
            else
                myalert('Der skete en fejl, beklager.');
        },
        error: function (er) {
            myalert("error: " + er);
            $('.z-loading').fadeOut(400);
        }
    });
}

function fix_mobile() {
    var width = $(window).width();
    //alert(width_old + ' ' + width);
    if (Math.max(width_old, width) - Math.min(width_old, width) > 10) {
        width_old = width;
        //alert('ok');
    }
    else {
        //alert('not');
        return;
    }

    $(".remove_phone_block").css({ "display": "block" });
    $(".remove_phone_inline").css({ "display": "inline" });
    $(".remove_desk_block").css({ "display": "block" });
    $(".remove_desk_inline").css({ "display": "inline" });
    $(".remove_tab_block").css({ "display": "block" });
    $(".remove_tab_inline").css({ "display": "inline" });

    if (width <= 576) {
        //alert('below: w' + width);
        $(".remove_desk_block").remove();
        $(".remove_desk_inline").remove();
        $(".remove_tab_block").remove();
        $(".remove_tab_inline").remove();

        $(".content-body").removeClass('col-sm-10');
        $(".content-body").removeClass('col-sm-12');

        $(".content-body").addClass('col-sm-12');
    }
    else if (width > 576 && width <= 768) {
        //alert('between: w' + width);
        $(".remove_desk_block").remove();
        $(".remove_desk_inline").remove();
        $(".remove_phone_block").remove();
        $(".remove_phone_inline").remove();

        $(".content-body").removeClass('col-sm-10');
        $(".content-body").removeClass('col-sm-12');

        $(".content-body").addClass('col-sm-12');

        fix_booths(1);
    }
    else if (width > 768 && width <= 992) {
        //alert('between: w' + width);
        $(".remove_desk_block").remove();
        $(".remove_desk_inline").remove();
        $(".remove_phone_block").remove();
        $(".remove_phone_inline").remove();

        $(".content-body").removeClass('col-sm-10');
        $(".content-body").removeClass('col-sm-12');

        $(".content-body").addClass('col-sm-12');

        fix_booths(2);
    }
    else if (width > 992 && width <= 1200) {
        //alert('above: w' + width);
        $(".remove_phone_block").remove();
        $(".remove_phone_inline").remove();
        $(".remove_tab_block").remove();
        $(".remove_tab_inline").remove();

        $(".content-body").removeClass('col-sm-10');
        $(".content-body").removeClass('col-sm-12');

        $(".content-body").addClass('col-sm-10');

        $('.l-al-slogan').hide();
        
        fix_booths(2);
    }
    else {
        //alert('above: w' + width);
        $(".remove_phone_block").remove();
        $(".remove_phone_inline").remove();
        $(".remove_tab_block").remove();
        $(".remove_tab_inline").remove();

        $(".content-body").removeClass('col-sm-10');
        $(".content-body").removeClass('col-sm-12');

        $(".content-body").addClass('col-sm-10');

        $('.l-al-slogan').css('display', 'inline-block');

        fix_booths(3);
    }

    if (width <= 576) {
        //alert('below');
        $("body").removeClass('z-margin-top-50');
        $("body").css('margin-top', '0');
    }
    else {

    }


}

function fix_booths(numbers) {
    var counter = 0;
    $('.row1').html('');
    $('.booth-item').each(function () {
        $('.row1').append($(this).html());
    });
    $('.booth-items').hide();
    $('.b-item').removeClass('col-xs-4');
    $('.b-item').removeClass('col-xs-6');
    $('.b-item').removeClass('col-xs-12');
    if (numbers == 3) {
        $('.b-item').addClass('col-xs-4');
    }
    if (numbers == 2) {
        $('.b-item').addClass('col-xs-6');
    }
    if (numbers == 1) {

        $('.b-item').addClass('col-xs-12');
    }
}

function just_remove() {
    
    $(".z-just-remove").remove();
}

var one_refresh = true;
function myRefresh() {
    //alert('fresh');
    var headers = $('.header').length
    if (one_refresh && headers == 0) {
        one_refresh = false;
        myalert('opdaterer skærm vidde..');
        location.reload();
    }
} 










/*
 * Arcade 
 * */

var loading_enter = false;
//$(window).on("load", function () {
//    var downloadingImages = [];
//    var counter = 0;
//    $(".loadlater").each(function (index, element) {
//        downloadingImages[counter] = new Image();
//        downloadingImages[counter].onload = function () {
//            $(element).attr("src", this.src);
//        };
//        downloadingImages[counter].src = $(element).data("src");
//        counter++;
//    });
//});
$(document).keypress(function (event) {
    var keycode = (event.keyCode ? event.keyCode : event.which);
    if (keycode == '13' && !loading_enter) {
        loading_enter = true;
        try {
            //alert('ind');
            setupSearch();
            actionFunc();
            //alert('ud');
            $('.z-loading').fadeIn(400);
        }
        catch (e) {}
        //catch (e)
        //{
        //    alert('ind2');
        //    if ($('msg_txt_comment').val() != '') {
        //        alert('ind3');
        //        $('form').submit();
        //        //e.preventDefault();
        //        //return false;
        //    }
        //    alert('ud2');
        //}
    }
});

function ready_arc() {
    //alert('hep');
    showMessages();

    $('.mytooltip_showcase').mouseenter(function () {
        $(this).find('span').css('position', 'absolute');
        $(this).find('span').css('display', 'inline');
    });
    $('.mytooltip_showcase').mouseleave(function () {
        $(this).find('span').css('position', 'absolute');
        $(this).find('span').css('display', 'none');
    });
    $('.zip_input').keyup(function () {
        if ($(this).val().length > 4)
            $(this).val($(this).val().substr(0, 4));
        $('.area_input').children().each(function () {
            $(this).removeAttr('selected');
        });
        $('.area_input option:contains(dk)').prop('selected', true);
    });
    $('.area_input').change(function () {
        $('.zip_input').val('');
        var selected = $(".area_input option:selected").text();
        if (selected.substr(0, 4) == 'dk' && selected.length > 4) {
            $('.area_input option:contains(dk)').removeAttr('selected');
        }
    });
    $('#from').keyup(function () {
        if ($(this).val() != '' || $('#to').val() != '') {
            $('.kun_med_fast_input').prop('checked', true);
        }
        else {
            $('.kun_med_fast_input').prop('checked', false);
        }
    });
    $('#to').keyup(function () {
        if ($(this).val() != '' || $('#from').val() != '')
            $('.kun_med_fast_input').prop('checked', true);
        else
            $('.kun_med_fast_input').prop('checked', false);
    });
    $('.area_check').click(function () {
        //alert($('#area_checked').val());
        if ($(this).prop('checked')) {
            var tmp = myDecode($('#area_checked').val());
            tmp = tmp == '-' ? '' : tmp;
            var area_checked = tmp + $(this).closest('span').find('span').text() + '-';
            //alert(area_checked);
            area_checked = area_checked.replace('dk-', '');
            area_checked = area_checked.replace('dk', '');
            window.location.href = $('#url').val() + '&area=' + format(area_checked);
        }
        else {
            var area_checked = myDecode($('#area_checked').val());
            area_checked = removeFromString(area_checked, format($(this).closest('span').find('span').text()), '-');
            if (area_checked == '') {
                $('.area_input').children().each(function () {
                    $(this).removeAttr('selected');
                });
                window.location.href = $('#url').val() + '';
            }
            else {
                area_checked = area_checked.replace('dk-', '');
                area_checked = area_checked.replace('dk', '');
                window.location.href = $('#url').val() + '&area=' + format(area_checked);
            }
        }
    });


    $('.tag_btn').click(function fun() {
        var tag = $(this).attr('name') + ';';
        search(tag);
    });

    $('.z-myalert-btn').click(function () {
        $('.z-myalert').fadeOut(200);
        $('.z-loading').fadeOut(200);
    });

    var nav_show = false;
    $('.z-nav').hide();
    $('button.z-nav-toggle').click(function () {
        nav_show = !nav_show;
        if (nav_show)
            $('.z-nav').show();
        else
            $('.z-nav').hide();
    });

    var src_show = false;
    $('.z-src').hide();
    $('button.z-src-toggle').click(function () {
        src_show = !src_show;
        if (src_show)
            $('.z-src').show();
        else
            $('.z-src').hide();
    });
}









/*
 * Navigation
 * */

var arr = [];
var cat_top = '';
var cat_sub = '';
var cat_sub_for_hover = '';

function ready_nav() {
    cat_top = $('#cata_sel').val() == '' ? '' : $('#cata_sel').val();
    cat_sub = $('#catb_sel').val() == '' ? '' : $('#catb_sel').val();
    cat_sub_for_hover = cat_sub;

    var path = '/Marketplace/GetCats';
    var data = {};
    data.ok = 'ok';
    getCats_Ajax(path, data);

    $('.cat_sec').hover(function (e) {
        if ($(this).data('catb') == cat_sub_for_hover)
            $('.params_content').each(function () {
                if (cat_top == $(this).closest('div').data('cata') && $(this).data('catb') == cat_sub_for_hover) {
                    //alert('hep:' + cat_sub_for_hover);
                    //$(this).removeClass('z-display-none');
                    $(this).fadeIn(400);
                    //if ($(window).width() > 768)
                    //    $('.z-loading').fadeIn(400);
                }
            });

    });
}

function params_hide() {
    //alert('godt');
    $('.params_content').fadeOut(400);
    $('.z-loading').fadeOut(400);
    run = true;
}

function Utf8_ansi_replace(val) {
    if (val == '')
        return '';

    var arr = [
        //["&#xc0", "À"],//["&#xc1", "Á"],//["&#xc2", "Â"],//["&#xc3", "Ã"],//["&#xc4", "Ä"],
        ["&#xC5", "Å"],["&#xC6", "Æ"],
        //["&#xc7", "Ç"],//["&#xc8", "È"],//["&#xc9", "É"],//["&#xca", "Ê"],//["&#xcb", "Ë"],//["&#xcc", "Ì"],//["&#xcd", "Í"],//["&#xce", "Î"],//["&#xcf", "Ï"],//["&#xd1", "Ñ"],//["&#xd2", "Ò"],//["&#xd3", "Ó"],//["&#xd4", "Ô"],//["&#xd5", "Õ"],//["&#xd6", "Ö"],
        ["&#xD8", "Ø"],
        //["&#xd9", "Ù"],//["&#xda", "Ú"],//["&#xdb", "Û"],//["&#xdc", "Ü"],//["&#xdd", "Ý"],//["&#xdf", "ß"],//["&#xe0", "à"],//["&#xe1", "á"],//["&#xe2", "â"],//["&#xe3", "ã"],//["&#xe4", "ä"],
        ["&#xE5", "å"],["&#xE6", "æ"],
        //["&#xe7", "ç"],//["&#xe8", "è"],//["&#xe9", "é"],//["&#xea", "ê"],//["&#xeb", "ë"],//["&#xec", "ì"],//["&#xed", "í"],//["&#xee", "î"],//["&#xef", "ï"],//["&#xf0", "ð"],//["&#xf1", "ñ"],//["&#xf2", "ò"],//["&#xf3", "ó"],//["&#xf4", "ô"],//["&#xf5", "õ"],//["&#xf6", "ö"],
        ["&#xF8", "ø"],
        //["&#xf9", "ù"],//["&#xfa", "ú"],//["&#xfb", "û"],//["&#xfc", "ü"],//["&#xfd", "ý"],//["&#xff", "ÿ"]
    ];

    for (var i = 0; i < arr.length; i++) {
        while (val.includes(arr[i][0]))
            val = val.replace(arr[i][0], arr[i][1]).replace(';', '');
    }

    return val;

}

function format(str) {
    if (str == '')
        return '';
    str = str.replace('æ', 'ae').replace('æ', 'ae').replace('ø', 'oe').replace('ø', 'oe').replace('å', 'aa').replace('å', 'aa').replace(' ', '_').replace(' ', '_');
    return str;
}

function fillParams() {
    //alert('yes');
    var cat_s_oe = format(Utf8_ansi_replace(cat_sub));
    if (cat_s_oe == '')
        return;
    //alert('no');
    var m = '';
    var s = '';
    var prev_parent = '';

    $('.m.param_chk.' + cat_s_oe).each(function () {

        if (cat_top == $(this).closest('div').data('cata')) {
            var parent = '';
            var child = '';
            var _new = false;
            if (!($(window).width() <= 768)/*$('#is_mob').val()*/) {

                parent = $(this).closest('td').prev('td').find('span').text().replace(':', '').toLowerCase().trim();
            }
            else {

                parent = $(this).closest('table').find('tr').find('span').text().replace(':', '').toLowerCase().trim();
            }

            if ($(this).prop('checked')) {
                child = '1';
            }
            else
                child = '0';

            if (parent != prev_parent) {
                m += m == '' ? '' : '_';
                prev_parent = parent;
                _new = true;
            }

            m += _new ? child : '\:' + child;
        }
    });

    prev_parent = '';
    $('.s.param_chk.' + cat_s_oe).each(function () {

        var child = '';

        if ($(this).prop('checked')) {
            child = '1';
        }
        else
            child = '0';

        s += s == '' ? child : '_' + child;
    });

    //alert(s + ' : ' + m);
    if(m != '')
        m = s != '' ? '_' + m : m;
    for (var i = 0; i < arr.length; i++) {
        if (arr[i].sub == format(cat_top) + '_' + cat_s_oe) {
            //alert(s + ' : ' + m);

            arr[i].par = s + m;
        }
    }
}

function setupSearch() {

    var search = $('.search_input').val() != '' ? $('.search_input').val() : '';
    var area = $('.area_input').val() != '' ? $('.area_input').val() : 'dk';
    var zip = $('.zip_input').val() != '' ? $('.zip_input').val() : '0';
    var fra = $('.fra_input').val() != '' ? $('.fra_input').val() : '0';
    var til = $('.til_input').val() != '' ? $('.til_input').val() : '999999';
    //var gra = $('.kun_med_fast_input').prop('checked') ? 'true' : 'false';

    $('#s').val(search);
    $('#a').val(area);
    $('#z').val(zip);
    $('#f').val(fra);
    $('#t').val(til);
    //$('#gra').val(gra);
}

function actionFunc() {
    //alert('hep');
    if (cat_sub != 'empty')
        fillParams();

    var c = '';
    var param = '';

    var top = format(Utf8_ansi_replace(cat_top.replace(' ', '_').replace(' ', '_')));//quick fix: special replace * 2, dont know why, maybe error in Utf8_ansi_replace

    var sub = 'empty';
    if (typeof cat_sub !== 'undefined') {

        sub = format(Utf8_ansi_replace(cat_sub));
    }
        //alert(top + ' ' + sub);

    for (var i = 0; i < arr.length; i++) {

        if (arr[i].top == top && arr[i].sub == 'empty') {

            c = arr[i].c != '' ? 'c=' + arr[i].c : '';
            param = arr[i].par != '' ? '&p=' + arr[i].par : '';
        }
        else if (sub != 'empty' && arr[i].sub == top + '_' + sub) {

            c = arr[i].c != '' ? 'c=' + arr[i].c : '';
            param = arr[i].par != '' ? '&p=' + arr[i].par : '';
        }
    }

    var s = $('#s').val() != '' ? '&s=' + $('#s').val() : '';
    var f = $('#f').val() != '0' ? '&f=' + $('#f').val() : '';
    var t = $('#t').val() != '999999' ? '&t=' + $('#t').val() : '';
    var z = $('#z').val() != '0' ? '&z=' + $('#z').val() : '';
    var a = $('#a').val() != '' ? '&a=' + $('#a').val() : '';
    //var g = $('#gra').val() != 'false' ? '&gra=' + $('#gra').val() : '';
    //alert(param);
    var query = c + param + s + f + t + z + /*g + */a;

    if (query.length > 0 && query.substring(0, 1) == '&')
        query = query.replace('&', '');

    window.location.href = $('#nav_lnk1').val() + '?' + query;
}

function cleanParameters() {
    $('.param_chk').each(function () {
        $(this).prop('checked', false);
    });
}

function cleanParametersInArray() {
    for (var i = 0; i < arr.length; i++) {
        arr[i].par = '';
    }
}

var run = true;
function cata(me, mobile) {
    if (!run || run == 'undefined')
        return;

    //alert('cata');
    var cata = $(me).data('cata');

    cat_top = cata;
    cat_sub = '';

    var me_name = $(me).data('cata');

    $('ul a i').each(function () {
        //alert('cata1');
        $(this).removeClass('fas fa-minus-square');
        $(this).addClass('fas fa-plus-square');
    });

    var is_visible = false;
    $('ul.catb').each(function () {
        //alert('cata2');
        if ($(this).data('catb') == me_name && !$(this).is(":visible")) {
            //alert('cata3' + me_name);
            if (!mobile)
                $('div.z-loading').show()

            $(this).removeClass('z-display-none');
            $(this).show(500);

            $(me).removeClass('fas fa-plus-square');
            $(me).addClass('fas fa-minus-square');
            is_visible = true;
        }
        else {
            $(this).addClass('z-display-none');
            $(this).hide(500);
        }
    });
    //alert('cata4');
    if (!mobile) {
        setupSearch();
        actionFunc();
    }
}

function catb(me, mobile) {
    //alert('hep');
    run = false;
    var cata = $(me).data('cata');
    var catb = $(me).data("catb");

    cat_top = cata;
    cat_sub = catb;
    if (mobile) {
        $('ul li.cat_sec').each(function () {
            if ($(this).data('cata') == cat_top && $(this).data('catb') == cat_sub) {
                $(this).find('span').css('color', 'white');
            }
            else {
                $(this).find('span').css('color', 'black');
            }
        });
        $('.params_content').each(function (j) {
            if ($(this).data('cata') == cata && $(this).data('catb') == catb) {
                $(this).show(500);
                
            }
            else
                $(this).hide(500);
        });
    }
    else {


        setupSearch();
        actionFunc();
    }
}

function getCats_Ajax(path, data_in) {
    $.ajax({
        url: path,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data_in),
        success: function (result) {
            if (result.success) {
                var obj = JSON.parse('' + result.arr);
                for (var i = 0; i < obj.list.length; i++) {
                    var json = { 'top': obj.list[i].top, 'sub': obj.list[i].sub, 'c': obj.list[i].value, 'par': '' };
                    arr.push(json);
                }
                //alert('ok');
            }
            else {
                //alert('nå');
                myalert('Der skete en fejl, beklager.', true);
            }
        },
        error: function (er) {
            ;//myalert("error1: " + er.responseText, true);
        }
    });
}









/*
 * Booth
 * */

var open_catelog = $('.hidden_catelog').val();

function ready_booth() {
    var rating = $('#rating').val();
    //alert(rating);
    setRating(rating);

    //$('.z-myalert').hide();
    $('.tag_btn').click(function fun() {
        var tag = $(this).attr('name');
        var role = $(this).attr('role');
        search(tag, role);
    });
    $('.fa-star').hover(function fun() {
        var id = $(this).attr('id');
        if (rating == -1)
            setRating(id);
    });
    $('.fa-star').click(function fun() {
        var id = $(this).attr('id');
        var booth_id = $('.bo_hidden_boothid').val();
        var person_id = $('.bo_hidden_personid').val();
        var path = $('#rating_url').val();
        var data = { 'rating': id, 'booth_id': booth_id, 'person_id': person_id };
        addRating_Ajax(path, data);
    });
    $('.cata').click(function () {
        if ($(this).closest('a').next().next().is(':visible')) {
            $(this).removeClass('fas fa-minus-square');
            $(this).addClass('fas fa-plus-square');
        }
        else {
            $(this).removeClass('fas fa-plus-square');
            $(this).addClass('fas fa-minus-square');
        }
        $(this).closest('a').next().next().toggle(500);

        $('.cata').not(this).each(function () {
            if ($(this).closest('a').next().next().is(':visible')) {
                $(this).removeClass('fas fa-minus-square');
                $(this).addClass('fas fa-plus-square');
            }
            $(this).closest('a').next().next().hide(500)
        })
    });
    $('.z-myalert-btn').click(function () {
        $('.z-myalert').fadeOut(200);
        $('.z-loading').fadeOut(200);
    });    
}

//function myalert(msg) {
//    fade = false;
//    $('.z-loading').fadeIn(400);
//    $('.z-myalert').fadeIn(400);
//    $('.z-myalert .z-myalert-txt').html(msg);
//}

function setRating(id) {
    for (var i = 1; i < 6; i++) {
        if (i <= id) {
            $('.fa-star#' + i).removeClass('far')
            $('.fa-star#' + i).addClass('fas')
        }
        else {
            $('.fa-star#' + i).addClass('far')
            $('.fa-star#' + i).removeClass('fas')
        }
    }
}

function search(str, role) {
    if (role == 'search')
        window.location.href = $('#marketplace_url').val() + '?s=' + str;
    else
        window.location.href = $('#marketplace_url').val() + '?c=' + str;
}

function addRating_Ajax(path, data_in) {
    $.ajax({
        url: path,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data_in),
        success: function (result) {
            if (result.success) {
                rating = result.rating;
                setRating(result.rating);
            }
            else
                myalert("Der skete en fejl, beklager.");
            return -1;
        },
        error: function (er) {
            myalert("error! error! " + er);
            return -1;
        }
    });
}