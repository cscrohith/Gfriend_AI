function isClickable(id) {
    elem = document.getElementById(id);
    if (elem.nodeName.toLowerCase() == 'a' || typeof (elem.click) != 'undefined') {
        return true;
    } else {
        return false;
    }
}

function isOutOfView(id) {
    var bounding = document.getElementById(id).getBoundingClientRect();
    var out = {};
    out.top = bounding.top < 0;
    out.left = bounding.left < 0;
    out.bottom = bounding.bottom > (window.innerHeight || document.documentElement.clientHeight);
    out.right = bounding.right > (window.innerWidth || document.documentElement.clientWidth);

    return out.top || out.left || out.bottom || out.right;
}

function getClientRectIsOutOfView(top, left, bottom, right) {
    var out = {};
    out.top = top < 0;
    out.left = left < 0;
    out.bottom = bottom > (window.innerHeight || document.documentElement.clientHeight);
    out.right = right > (window.innerWidth || document.documentElement.clientWidth);

    return out.top || out.left || out.bottom || out.right;
}

function getIsExistElementbyId(id) {
    var element = document.getElementById(id);
    if (element === null) {
        return false;
    }

    var bounding = document.getElementById(id).getBoundingClientRect();
    if (getClientRectIsOutOfView(bounding.top, bounding.left, bounding.bottom, bounding.right)) {
        return false;
    }

    return true;
}

function getIsExistElementbyCss(css) {
    var element = document.querySelector(css);
    if (element === null) {
        return false;
    }

    var bounding = document.querySelector(css).getBoundingClientRect();
    if (getClientRectIsOutOfView(bounding.top, bounding.left, bounding.bottom, bounding.right)) {
        return false;
    }

    return true;
}

function getIsExistElementbyXpath(xpath) {
    var element = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
    if (element === null) {
        return false;
    }

    var bounding = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.getBoundingClientRect();
    if (getClientRectIsOutOfView(bounding.top, bounding.left, bounding.bottom, bounding.right)) {
        return false;
    }

    return true;
}

function getBoudingClientRectbyXpath(xpath) {
    var element = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.getBoundingClientRect();
    return JSON.stringify(element);
}

function getElementbyText(text) {
    var elements = document.querySelectorAll("*");
    var element = "";

    for (i = 0; i < elements.length; i++) {
        if (elements[i].innerText && elements[i].innerText === text) {
            element = elements[i];
            break;
        }
    }

    if (element === "") {
        return undefined;
    }

    return element;
}

function getElementbyTextwithIndex(text, index) {
    var elements = document.querySelectorAll("*");
    var elementCount = 0;
    var element = "";

    for (i = 0; i < elements.length; i++) {
        if (elements[i].innerText && elements[i].innerText === text) {
            elementCount++;

            if (elementCount == index) {
                element = elements[i];
                break;
            }
        }
    }

    if (element === "") {
        return undefined;
    }

    return element;
}

function setRegularText(text) {
    var afterText = text.replace(/>/g, '&gt;').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/\"/g, '&quot;').replace(/\n/g, '<br />');
    return afterText;
}
