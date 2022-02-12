function fcfg2csv_convert() {
  str = document.fcfg2csv.original.value;
  var lines = str.split('\n')
  result = "";
  lines.forEach(element => {
    var equalPos = element.indexOf('=');
    if (equalPos == -1) {
      result += element + '\n';
    }
    else {
      var words = [];
      words.push(element.substr(0, equalPos));
      words.push(element.substr(equalPos + 1));
      result += words[0] + ','
      if (words[1].indexOf("@Variant") >= 0) {
        var start = words[1].indexOf('(') + 1
        var end = words[1].indexOf(')')
        var view = unescapeToView(words[1].substr(start, end - start))
        result += view.getFloat32(4, false).toFixed(3)
      }
      else if (words[1].indexOf('[') >= 0) {
        result += '"' + words[1] + '"'
      }
      else {
        result += words[1];
      }
      result += '\n'
    }
  });

  document.fcfg2csv.result.value = result;
  document.fcfg2csv.result.focus();
  document.fcfg2csv.result.select();

  return 0;
}

var unescapeToView = function (str) {
  var rslt, i, l;

  rslt = [];
  for (i = 0, l = str.length; i < l; i++) {
    if (str.charAt(i) === '\\') {
      i++;
      if (str.charAt(i) === '0') {
        rslt.push(0x00);
      }
      else if (str.charAt(i) === 'f') {
        rslt.push(0x0C);
      }
      else if (str.charAt(i) === 'a') {
          rslt.push(0x07);
      }
      else if (str.charAt(i) === 'r') {
          rslt.push(0x0D);
      }
      else if (str.charAt(i) === 'x') {
        var ch = str.charAt(i + 2);
        if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f') {
          rslt.push(Number('0x' + str.charAt(++i) + str.charAt(++i)));
        }
        else {
          rslt.push(Number('0x' + str.charAt(++i)));
        }
      }
    }
    else {
      rslt.push(str.charCodeAt(i))
    }
  }
  var buffer = new ArrayBuffer(rslt.length)
  var view = new DataView(buffer)
  for(i = 0; i < rslt.length; ++i) {
    view.setUint8(i, rslt[i])
  }
  return view;
};