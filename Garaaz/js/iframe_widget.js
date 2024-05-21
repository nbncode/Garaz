
function OripartsIFrame(){

    // this.target_url = "https://oriparts.bla";
    this.target_url = "https://oriparts.com/7";

    this.name = "Oriparts";

    this.container_selector;

    this.container;

    this.overlay;

    this.frame;

    this.options = {};

    this.password;

    this.callback;

    this.replacements_callback;

    this.searchWithHints;
}

OripartsIFrame.prototype.init = function (container_selector, password, options, callback, replacements_callback) {

    var that = this;

    this.options = options;
    this.password = password;
    this.callback = callback;
    this.replacements_callback = replacements_callback;
    this.container_selector = container_selector;

    this.createContainer();
    this.createFrame();
    that.frame.contentWindow.focus();

    that.frame.addEventListener("load", function (event) {

        that.frame.contentWindow.postMessage(
            {
                "type": "initDialog",
                "data": that.options
            },
            that.target_url
        );
    });

    window.addEventListener("message", function (event) {

        that.processResponse(event);

        that.postMessageHandler(event);

    });

    window.addEventListener('click', function(event){

        that.preClickHandler(event);

        that.getReplacements(event);
        that.addReplacement(event);

    });

};

OripartsIFrame.prototype.initSearchWithHints = function(selector){

    var that = this;

    $(document).on("getProductsByCarAndCategory", function (event, url, search) {

        that.frame.contentWindow.postMessage(
            {
                "type": "getProductsByCarAndCategory",
                "data": {
                    'url': url,
                }
            },
            that.target_url
        );

        search.clear();
    });

    $(document).on("submit", selector, function(event){

        event.preventDefault();
        event.stopPropagation();

        let query = $(event.target).find('input').eq(0).val().trim();

        if(typeof that.searchWithHints !== "undefined"){

            that.searchWithHints.destroy();
        }

        that.searchWithHints = new SearchWithHints($(event.target).find('input').eq(0), query);

    });
};

OripartsIFrame.prototype.initSearchByCode = function(selector){

    var that = this;

    document.querySelectorAll(selector).forEach(function(element){

        element.addEventListener('change submit', function(event){

            event.preventDefault();
            event.stopPropagation();

            var code = event.target.value.trim();

            if(code.length > 0){

                that.frame.contentWindow.postMessage(
                    {
                        "type": "getProductByCode",
                        "data": {
                            'code': code,
                        }
                    },
                    that.target_url
                );
            }
        });
    });
};

OripartsIFrame.prototype.preClickHandler = function(event){

    if(typeof event.target.dataset.oripartsIframeAction !== "undefined"){

        document.querySelectorAll("[data-oriparts-iframe-action]").forEach(function(element){

            element.disabled = true;
        });
    }
};

OripartsIFrame.prototype.postMessageHandler = function(event){

    document.querySelectorAll("[data-oriparts-iframe-action]").forEach(function(element){

        element.disabled = false;
    });

};

OripartsIFrame.prototype.addReplacement = function(event){

    var that = this;

    var element = event.target;

    if(element.dataset.oripartsIframeAction === "getReplacement"){

        that.frame.contentWindow.postMessage(
            {
                "type": "getReplacement",
                "data": {
                    'id': element.dataset.id
                }
            },
            that.target_url
        );
    }
};

OripartsIFrame.prototype.getReplacements = function(event){

    var that = this;

    var element = event.target;

    if(element.dataset.oripartsIframeAction === "getReplacements"){

        document.querySelectorAll("[role=dialog]").forEach(function(element){

            element.remove();
        });

        that.frame.contentWindow.postMessage(
            {
                "type": "getReplacements",
                "data": {
                    'id': element.dataset.id,
                    'is_b': element.dataset.is_b
                }
            },
            that.target_url
        );
    }
};

OripartsIFrame.prototype.processResponse = function (event) {

    var that = this;

    if(
        event.origin === that.target_url &&
        event.data.type !== "undefined" &&
        event.data.type === "response"
    ){

        var data = that.parseResponse(event.data.data);

        Object.assign(data, event.data.request_data);

        if(event.data.action === "getReplacements" || event.data.action === "getProductsByCarAndCategory"){

            if(typeof that.replacements_callback === "function"){
                that.replacements_callback(data);
            }
        }else{
            if(typeof this.callback === "function"){
                this.callback(data);
            }
        }
    }
};

OripartsIFrame.prototype.parseResponse = function(response){

    var data = response.replace(this.password, "");

    data = JSON.parse(atob(data));

    return data;
};

OripartsIFrame.prototype.createContainer = function() {

    var cont = document.getElementById(this.container);

    if (cont) {
        cont.parentElement.removeChild(cont);
    }
    this.container = document.createElement('div');
    this.container.style.cssText = 'z-index: 9997; height: 100%; width: 100%;';
    this.container.setAttribute('id', this.container);

    document.getElementById(this.container_selector).appendChild(this.container);

};

OripartsIFrame.prototype.createFrame = function() {

    this.frame = document.createElement('iframe');
    this.frame.setAttribute('name', this.name);
    this.frame.setAttribute('src', this.target_url + "/?utm_source=iframe");
    this.frame.setAttribute('frameborder', '0');
    this.frame.setAttribute('scrolling', 'yes');
    this.frame.style.cssText = 'background: transparent; z-index: 9999; width: 100%; height: 100%;background-image:url("https://s3.eu-central-1.amazonaws.com/w4p-merch/logo/widget_preloader_light.gif");background-position: center center;background-repeat: no-repeat;';
    this.container.appendChild(this.frame);

};

function SearchWithHints(selector, initialQuery){

    this.selector = selector;

    this.initialQuery = initialQuery;

    this.dataDomain = "//boodmo.com";

    this.endpoint = "https://service.boodmo.com/searchster/";

    this.response = {};

    this.initSearch();
}

SearchWithHints.prototype.initSearch = function(){

    let _this = this;

    _this.sendRequest(_this.endpoint + "?term=" + _this.initialQuery);

    $(document).on('click', '[data-search-with-hints-action]', function (event) {

        event.preventDefault();

        _this.sendRequest($(this).data('search-with-hints-action'));
    });
};

SearchWithHints.prototype.sendRequest = function(url){

    let _this = this;

    $.ajax(
        url,
        {
            success: function (data) {

                _this.response = data;

                _this.destroy();

                if(typeof data.result.answer !== "undefined"){

                    $(document).trigger("getProductsByCarAndCategory", [data.result.answer, _this]);
                }else{
                    _this.buildHint(data.result);
                }
            }
        }
    );
};

SearchWithHints.prototype.buildHint = function(data){

    if(this.response.length === 0){
        return;
    }

    let css = '<style>' +
        '.smartSearch__filter {\n' +
        '    display: flex;\n' +
        '    align-items: center;\n' +
        '    cursor: pointer;\n' +
        '    background-color: #f1f1f1;\n' +
        '    overflow: hidden;\n' +
        '    padding: 1px;\n' +
        '    margin: 7.5px;\n' +
        '    height: 35px;\n' +
        '}' +
        '.smartSearch__photo {\n' +
        '    display: block;\n' +
        '    max-width: 100%;\n' +
        '    max-height: 100%;\n' +
        '}' +
        '.smartSearch__text {\n' +
        '    display: flex;\n' +
        '    flex-direction: row;\n' +
        '    flex: 1;\n' +
        '    flex-basis: auto;\n' +
        '    justify-content: space-between;\n' +
        '    min-width: 0;\n' +
        '    align-items: center;\n' +
        '    color: #000;\n' +
        '    margin-left: 5px;\n' +
        '}' +
        '.smartSearch__close {\n' +
        '    cursor: pointer;\n' +
        '    color: #000;\n' +
        '    font-size: 21px;\n' +
        '    padding: 5px;\n' +
        '}' +
        '.smartSearch__filters {\n' +
        '    display: flex;\n' +
        '    flex-wrap: wrap;\n' +
        '    margin-bottom: 7.5px;\n' +
        '}' +
        '.smartSearch__popover {\n' +
        '    padding: 5px 12px;\n' +
        '    display: inline-block;\n' +
        '    background: #e59d37;\n' +
        '    border-radius: 6px;\n' +
        '    font-style: normal;\n' +
        '    font-weight: 400;\n' +
        '    font-size: 12px;\n' +
        '    line-height: 14px;\n' +
        '    color: #fff;\n' +
        '    position: relative;\n' +
        '    margin-bottom: 7.5px;\n' +
        '}' +
        '.smartSearch__popover.down:before {\n' +
        '    bottom: -6px;\n' +
        '    border-left: 5px solid transparent;\n' +
        '    border-right: 5px solid transparent;\n' +
        '    border-top: 6px solid #e59d37;\n' +
        '}\n' +
        '.smartSearch__popover:before {\n' +
        '    content: "";\n' +
        '    display: inline-block;\n' +
        '    position: absolute;\n' +
        '    right: calc(50% - 3px);\n' +
        '    overflow: hidden;\n' +
        '    height: 0;\n' +
        '    width: 0;\n' +
        '}' +
        '.smartSearch__suggest_inner {\n' +
        '    display: flex;\n' +
        '    flex-wrap: wrap;\n' +
        '}' +
        '.smartSearch__offer {\n' +
        '    border: 1px solid rgba(0,0,0,.15);\n' +
        '    box-sizing: border-box;\n' +
        '    padding: 6px;\n' +
        '    margin: 7.5px;\n' +
        '    text-align: center;\n' +
        '    flex: 1 0 auto;\n' +
        '    cursor: pointer;\n' +
        '}' +
        '.smartSearch__offer_title {\n' +
        '    max-width: 180px;\n' +
        '    display: inline-block;\n' +
        '    color: #000;\n' +
        '    vertical-align: middle;\n' +
        '}' +
        '</style>';

    $(this.selector).attr('data-html', true);
    $(this.selector).attr('data-placement', "auto");
    $(this.selector).attr('data-trigger', "manual");
    $(this.selector).attr('title', css + this.buildHintTemplate(data));

    $(this.selector).tooltip({
        template: '<div class="tooltip" role="tooltip" style="opacity: 1;">' +
            '<div class="arrow"></div>' +
            '<div class="tooltip-inner" style="background: white;border: 1px solid #000;max-width: 750px;width: initial;max-height: 400px;overflow-x: auto;"></div>' +
            '</div>'
    });

    $(this.selector).tooltip("show");
};

SearchWithHints.prototype.buildHintTemplate = function(data){

    let html = '<div class="wrapper">';

    if(typeof data.filters !== "undefined" && data.filters.length > 0){

        html += '<div class="smartSearch__filters">';

        for(let f in data.filters){

            let filter = data.filters[f];

            html += '<label class="smartSearch__filter" data-search-with-hints-action="https:' + filter.action.clear + '">';

            if(typeof filter.image !== "undefined"){

                html += '<img class="smartSearch__photo" width="57" height="35" src="' + this.dataDomain + filter.image + '" alt="' + filter.name + '">';
            }

            html += '<span class="smartSearch__text">' + filter.name + '</span>';
            html += '<div class="smartSearch__close" aria-hidden="true">×</div>';
            html += '</label>';
        }

        html += '</div>';
    }

    if(typeof data.question !== "undefined"){

        html += '<div class="text-center"><div class="smartSearch__popover down">' + data.question.content + '</div></div>';

        if(typeof data.question.options !== "undefined" && data.question.options.length > 0){

            html += '<div class="smartSearch__suggest_inner">';

            for(let o in data.question.options){

                let option = data.question.options[o];

                html += '<div class="smartSearch__offer" data-search-with-hints-action="https:' + option.action.select + '">';

                if(typeof option.image !== "undefined"){

                    html += '<img width="125" height="76" src="' + this.dataDomain + option.image + '" alt="' + option.content + '">';
                }

                if(data.question.queryName !== 'brand'){
                    html += '<span class="smartSearch__offer_title">' + option.content + '</span>';
                }

                html += '</div>';
            }

            html += '</div>';
        }
    }

    html += '</div>';
    console.log(html);
    return html;
};

SearchWithHints.prototype.destroy = function () {
    $(this.selector).tooltip("dispose");
};

SearchWithHints.prototype.clear = function () {
    $(this.selector).val('');
    this.destroy();
};