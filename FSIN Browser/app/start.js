(function () {
  var form = document.getElementById("searchForm");
  var input = document.getElementById("searchInput");

  function toUrl(value) {
    var text = (value || "").trim();
    if (!text) {
      return "";
    }

    if (/^(https?:\/\/|file:\/\/)/i.test(text)) {
      return text;
    }

    if (/^[\w.-]+\.[a-z]{2,}(?::\d+)?(?:\/.*)?$/i.test(text)) {
      return "https://" + text;
    }

    return "https://www.bing.com/search?q=" + encodeURIComponent(text);
  }

  form.addEventListener("submit", function (event) {
    event.preventDefault();
    var url = toUrl(input.value);
    if (url) {
      window.location.href = url;
    }
  });

  input.focus();
}());
