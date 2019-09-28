// SaltarelleCompiler Runtime (http://www.saltarelle-compiler.com)
// Modified version of Script# Core Runtime (http://projects.nikhilk.net/ScriptSharp)

if (typeof (global) === 'undefined') {
  if (typeof (window) !== 'undefined')
    global = window;
  else if (typeof (self) !== 'undefined')
    global = self;
}
(function (global) {
  'use strict';

  var ss = { __assemblies: {} };

  ss.initAssembly = function (obj, name, res) {
    res = res || {};
    obj.name = name;
    obj.toString = function () { return this.name; };
    obj.__types = {};
    obj.getResourceNames = function () { return Object.keys(res); };
    obj.getResourceDataBase64 = function (name) { return res[name] || null; };
    obj.getResourceData = function (name) { var r = res[name]; return r ? ss.dec64(r) : null; };
    ss.__assemblies[name] = obj;
  };
  ss.initAssembly(ss, 'mscorlib');

  ss.load = function (name) {
    return ss.__assemblies[name] || require(name);
  };

  var enc = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/', dec;
  ss.enc64 = function (a, b) {
    var s = '', i;
    for (i = 0; i < a.length; i += 3) {
      var c1 = a[i], c2 = a[i + 1], c3 = a[i + 2];
      s += (b && i && !(i % 57) ? '\n' : '') + enc[c1 >> 2] + enc[((c1 & 3) << 4) | (c2 >> 4)] + (i < a.length - 1 ? enc[((c2 & 15) << 2) | (c3 >> 6)] : '=') + (i < a.length - 2 ? enc[c3 & 63] : '=');
    }
    return s;
  };

  ss.dec64 = function (s) {
    s = s.replace(/\s/g, '');
    dec = dec || (function () { var o = { '=': -1 }; for (var i = 0; i < 64; i++) o[enc[i]] = i; return o; })();
    var a = Array(Math.max(s.length * 3 / 4 - 2, 0)), i;
    for (i = 0; i < s.length; i += 4) {
      var j = i * 3 / 4, c1 = dec[s[i]], c2 = dec[s[i + 1]], c3 = dec[s[i + 2]], c4 = dec[s[i + 3]];
      a[j] = (c1 << 2) | (c2 >> 4);
      if (c3 >= 0) a[j + 1] = ((c2 & 15) << 4) | (c3 >> 2);
      if (c4 >= 0) a[j + 2] = ((c3 & 3) << 6) | c4;
    }
    return a;
  };

  ss.getAssemblies = function () {
    return Object.keys(ss.__assemblies).map(function (n) { return ss.__assemblies[n]; });
  };

  ss.isNullOrUndefined = function (o) {
    return (o === null) || (o === undefined);
  };

  ss.isValue = function (o) {
    return (o !== null) && (o !== undefined);
  };

  ss.referenceEquals = function (a, b) {
    return ss.isValue(a) ? a === b : !ss.isValue(b);
  };

  ss.mkdict = function () {
    var a = (arguments.length !== 1 ? arguments : arguments[0]);
    var r = {};
    for (var i = 0; i < a.length; i += 2) {
      r[a[i]] = a[i + 1];
    }
    return r;
  };

  ss.clone = function (t, o) {
    return o ? t.$clone(o) : o;
  }

  ss.coalesce = function (a, b) {
    return ss.isValue(a) ? a : b;
  };

  ss.isDate = function (obj) {
    return Object.prototype.toString.call(obj) === '[object Date]';
  };

  ss.isArray = function (obj) {
    return Object.prototype.toString.call(obj) === '[object Array]';
  };

  ss.isTypedArrayType = function (type) {
    return ['Float32Array', 'Float64Array', 'Int8Array', 'Int16Array', 'Int32Array', 'Uint8Array', 'Uint16Array', 'Uint32Array', 'Uint8ClampedArray'].indexOf(ss.getTypeFullName(type)) >= 0;
  };

  ss.isArrayOrTypedArray = function (obj) {
    return ss.isArray(obj) || ss.isTypedArrayType(ss.getInstanceType(obj));
  };

  ss.getHashCode = function (obj) {
    if (!ss.isValue(obj))
      throw new ss_NullReferenceException('Cannot get hash code of null');
    else if (typeof (obj.getHashCode) === 'function')
      return obj.getHashCode();
    else if (typeof (obj) === 'boolean') {
      return obj ? 1 : 0;
    }
    else if (typeof (obj) === 'number') {
      var s = obj.toExponential();
      s = s.substr(0, s.indexOf('e'));
      return parseInt(s.replace('.', ''), 10) & 0xffffffff;
    }
    else if (typeof (obj) === 'string') {
      var res = 0;
      for (var i = 0; i < obj.length; i++)
        res = (res * 31 + obj.charCodeAt(i)) & 0xffffffff;
      return res;
    }
    else if (ss.isDate(obj)) {
      return obj.valueOf() & 0xffffffff;
    }
    else {
      return ss.defaultHashCode(obj);
    }
  };

  ss.defaultHashCode = function (obj) {
    return obj.$__hashCode__ || (obj.$__hashCode__ = (Math.random() * 0x100000000) | 0);
  };

  ss.equals = function (a, b) {
    if (!ss.isValue(a))
      throw new ss_NullReferenceException('Object is null');
    else if (a !== ss && typeof (a.equals) === 'function')
      return a.equals(b);
    if (ss.isDate(a) && ss.isDate(b))
      return a.valueOf() === b.valueOf();
    else if (typeof (a) === 'function' && typeof (b) === 'function')
      return ss.delegateEquals(a, b);
    else if (ss.isNullOrUndefined(a) && ss.isNullOrUndefined(b))
      return true;
    else
      return a === b;
  };

  ss.compare = function (a, b) {
    if (!ss.isValue(a))
      throw new ss_NullReferenceException('Object is null');
    else if (typeof (a) === 'number' || typeof (a) === 'string' || typeof (a) === 'boolean')
      return ss.isValue(b) ? (a < b ? -1 : (a > b ? 1 : 0)) : 1;
    else if (ss.isDate(a))
      return ss.isValue(b) ? ss.compare(a.valueOf(), b.valueOf()) : 1;
    else
      return a.compareTo(b);
  };

  ss.equalsT = function (a, b) {
    if (!ss.isValue(a))
      throw new ss_NullReferenceException('Object is null');
    else if (typeof (a) === 'number' || typeof (a) === 'string' || typeof (a) === 'boolean')
      return a === b;
    else if (ss.isDate(a))
      return a.valueOf() === b.valueOf();
    else
      return a.equalsT(b);
  };

  ss.staticEquals = function (a, b) {
    if (!ss.isValue(a))
      return !ss.isValue(b);
    else
      return ss.isValue(b) ? ss.equals(a, b) : false;
  };

  ss.shallowCopy = (function () { try { var x = Object.getOwnPropertyDescriptor({ a: 0 }, 'a').value; return true; } catch (ex) { return false; } })() ?
    function (source, target) {
      var keys = Object.keys(source);
      for (var i = 0, l = keys.length; i < l; i++) {
        Object.defineProperty(target, keys[i], Object.getOwnPropertyDescriptor(source, keys[i]));
      }
    } :
    function (source, target) {
      var keys = Object.keys(source);
      for (var i = 0, l = keys.length; i < l; i++) {
        target[keys[i]] = source[keys[i]];
      }
    };

  ss.isLower = function (c) {
    var s = String.fromCharCode(c);
    return s === s.toLowerCase() && s !== s.toUpperCase();
  };

  ss.isUpper = function (c) {
    var s = String.fromCharCode(c);
    return s !== s.toLowerCase() && s === s.toUpperCase();
  };

  if (typeof (window) == 'object') {
    // Browser-specific stuff that could go into the Web assembly, but that assembly does not have an associated JS file.
    if (!window.Element) {
      // IE does not have an Element constructor. This implementation should make casting to elements work.
      window.Element = function () { };
      window.Element.isInstanceOfType = function (instance) { return instance && typeof instance.constructor === 'undefined' && typeof instance.tagName === 'string'; };
    }
    window.Element.__typeName = 'Element';

    ss.parseXml = function (markup) {
      var domParser = new DOMParser();
      return domParser.parseFromString(markup, 'text/xml');
    };
  }

///////////////////////////////////////////////////////////////////////////////
// Object Extensions

ss.clearKeys = function (d) {
  for (var n in d) {
    if (d.hasOwnProperty(n))
      delete d[n];
  }
};

ss.keyExists = function (d, key) {
  return d[key] !== undefined;
};

if (!Object.keys) {
  Object.keys = (function () {
    'use strict';
    var hasOwnProperty = Object.prototype.hasOwnProperty,
      hasDontEnumBug = !({ toString: null }).propertyIsEnumerable('toString'),
      dontEnums = ['toString', 'toLocaleString', 'valueOf', 'hasOwnProperty', 'isPrototypeOf', 'propertyIsEnumerable', 'constructor'],
      dontEnumsLength = dontEnums.length;

    return function (obj) {
      if (typeof obj !== 'object' && (typeof obj !== 'function' || obj === null)) {
        throw new TypeError('Object.keys called on non-object');
      }

      var result = [], prop, i;

      for (prop in obj) {
        if (hasOwnProperty.call(obj, prop)) {
          result.push(prop);
        }
      }

      if (hasDontEnumBug) {
        for (i = 0; i < dontEnumsLength; i++) {
          if (hasOwnProperty.call(obj, dontEnums[i])) {
            result.push(dontEnums[i]);
          }
        }
      }
      return result;
    };
  }());
}

ss.getKeyCount = function (d) {
  return Object.keys(d).length;
};

///////////////////////////////////////////////////////////////////////////////
// Type System Implementation

ss.__genericCache = {};

ss._makeGenericTypeName = function (genericType, typeArguments) {
  var result = ss.getTypeFullName(genericType);
  for (var i = 0; i < typeArguments.length; i++)
    result += (i === 0 ? '[' : ',') + '[' + ss.getTypeFullName(typeArguments[i]) + ']';
  result += ']';
  return result;
};

ss.makeGenericType = function (genericType, typeArguments) {
  var name = ss._makeGenericTypeName(genericType, typeArguments);
  return ss.__genericCache[ss._makeQName(name, genericType.__assembly)] || genericType.apply(null, typeArguments);
};

ss._registerGenericInstance = function (genericType, typeArguments, instance, members, statics, init) {
  if (!instance) {
    instance = function () { };
  }
  var name = ss._makeGenericTypeName(genericType, typeArguments);
  ss.__genericCache[ss._makeQName(name, genericType.__assembly)] = instance;
  instance.__typeName = name;
  instance.__assembly = genericType.__assembly;
  instance.__genericTypeDefinition = genericType;
  instance.__typeArguments = typeArguments;
  if (statics) {
    ss.shallowCopy(statics, instance);
  }
  init(instance);
  if (members) {
    ss.shallowCopy(members, instance.prototype);
  }
  return instance;
};

ss.registerGenericClassInstance = function (genericType, typeArguments, instance, members, statics, baseType, getInterfaceTypesFunc) {
  return ss._registerGenericInstance(genericType,
    typeArguments,
    instance,
    members,
    statics,
    function (inst) {
      ss.initClass(inst, baseType ? baseType() : null, getInterfaceTypesFunc ? getInterfaceTypesFunc() : null);
    });
};

ss.registerGenericStructInstance = function (genericType, typeArguments, instance, members, statics, getInterfaceTypesFunc) {
  return ss._registerGenericInstance(genericType,
    typeArguments,
    instance,
    members,
    statics,
    function (inst) { ss.initStruct(inst, getInterfaceTypesFunc ? getInterfaceTypesFunc() : null); });
};

ss.registerGenericInterfaceInstance = function (genericType, typeArguments, members, getBaseInterfacesFunc) {
  return ss._registerGenericInstance(genericType,
    typeArguments,
    null,
    members,
    null,
    function (instance) {
      ss.initInterface(instance, members, getBaseInterfacesFunc ? getBaseInterfacesFunc() : null);
    });
};

ss.isGenericTypeDefinition = function (type) {
  return type.__isGenericTypeDefinition || false;
};

ss.getGenericTypeDefinition = function (type) {
  return type.__genericTypeDefinition || null;
};

ss.getGenericParameterCount = function (type) {
  return type.__typeArgumentCount || 0;
};

ss.getGenericArguments = function (type) {
  return type.__typeArguments || null;
};

ss.__anonymousCache = {};
ss.anonymousType = function () {
  var members = Array.prototype.slice.call(arguments);
  var name = 'Anonymous<' + members.map(function (m) { return m[1] + ':' + ss.getTypeFullName(m[0]); }).join(',') + '>';
  var type = ss.__anonymousCache[name];
  if (!type) {
    type = new Function(members.map(function (m) { return m[1]; }).join(','),
      members.map(function (m) { return 'this.' + m[1] + '=' + m[1] + ';'; }).join(''));
    type.__typeName = name;
    var infos = members.map(function (m) {
      return {
        name: m[1],
        typeDef: type,
        type: 16,
        returnType: m[0],
        getter: { name: 'get_' + m[1], typeDef: type, params: [], returnType: m[0], fget: m[1] }
      };
    });
    infos.push({ name: '.ctor', typeDef: type, type: 1, params: members.map(function (m) { return m[0]; }) });
    type.__metadata = { members: infos };
    ss.__anonymousCache[name] = type;
  }
  return type;
}

ss.setMetadata = function (type, metadata) {
  if (metadata.members) {
    for (var i = 0; i < metadata.members.length; i++) {
      var m = metadata.members[i];
      m.typeDef = type;
      if (m.adder) m.adder.typeDef = type;
      if (m.remover) m.remover.typeDef = type;
      if (m.getter) m.getter.typeDef = type;
      if (m.setter) m.setter.typeDef = type;
    }
  }
  type.__metadata = metadata;
  if (metadata.variance) {
    type.isAssignableFrom = function (source) {
      var check = function (target, type) {
        if (type.__genericTypeDefinition === target.__genericTypeDefinition && type.__typeArguments.length === target.__typeArguments.length) {
          for (var i = 0; i < target.__typeArguments.length; i++) {
            var v = target.__metadata.variance[i], t = target.__typeArguments[i], s = type.__typeArguments[i];
            switch (v) {
              case 1: if (!ss.isAssignableFrom(t, s)) return false; break;
              case 2: if (!ss.isAssignableFrom(s, t)) return false; break;
              default: if (s !== t) return false;
            }
          }
          return true;
        }
        return false;
      };

      if (source.__interface && check(this, source))
        return true;
      var ifs = ss.getInterfaces(source);
      for (var i = 0; i < ifs.length; i++) {
        if (ifs[i] === this || check(this, ifs[i]))
          return true;
      }
      return false;
    };
  }
};

ss.mkType = function (asm, typeName, ctor, members, statics) {
  if (!ctor) ctor = function () { };
  ctor.__assembly = asm;
  ctor.__typeName = typeName;
  if (asm)
    asm.__types[typeName] = ctor;
  if (members) ctor.__members = members;
  if (statics) ss.shallowCopy(statics, ctor);
  return ctor;
};

ss.mkEnum = function (asm, typeName, values, namedValues) {
  var result = ss.mkType(asm, typeName);
  ss.shallowCopy(values, result.prototype);
  result.__enum = true;
  result.getDefaultValue = result.createInstance = function () { return namedValues ? null : 0; };
  result.isInstanceOfType = function (instance) { return typeof instance === (namedValues ? 'string' : 'number'); };
  return result;
};

ss.initClass = function (ctor, baseType, interfaces) {
  ctor.__class = true;
  if (baseType && baseType !== Object) {
    var f = function () { };
    f.prototype = baseType.prototype;
    ctor.prototype = new f();
    ctor.prototype.constructor = ctor;
  }
  if (ctor.__members) {
    ss.shallowCopy(ctor.__members, ctor.prototype);
    delete ctor.__members;
  }
  if (interfaces)
    ctor.__interfaces = interfaces;
};

ss.initStruct = function (ctor, interfaces) {
  ss.initClass(ctor, null, interfaces);
  ctor.__class = false;
  ctor.getDefaultValue = ctor.getDefaultValue || ctor.createInstance || function () { return new ctor(); };
};

ss.initGenericClass = function (ctor, typeArgumentCount) {
  ctor.__class = true;
  ctor.__typeArgumentCount = typeArgumentCount;
  ctor.__isGenericTypeDefinition = true;
};

ss.initGenericStruct = function (ctor, typeArgumentCount) {
  ss.initGenericClass(ctor, typeArgumentCount);
  ctor.__class = false;
};

ss.initInterface = function (ctor, members, baseInterfaces) {
  ctor.__interface = true;
  if (baseInterfaces) {
    ctor.__interfaces = baseInterfaces;
  }
  ss.shallowCopy(members, ctor.prototype);
  ctor.isAssignableFrom = function (type) { return ss.contains(ss.getInterfaces(type), this); };
};

ss.initGenericInterface = function (ctor, typeArgumentCount) {
  ctor.__interface = true;
  ctor.__typeArgumentCount = typeArgumentCount;
  ctor.__isGenericTypeDefinition = true;
};

ss.getBaseType = function (type) {
  if (type === Object || type.__interface) {
    return null;
  }
  else if (Object.getPrototypeOf) {
    return Object.getPrototypeOf(type.prototype).constructor;
  }
  else {
    var p = type.prototype;
    if (Object.prototype.hasOwnProperty.call(p, 'constructor')) {
      var ownValue = p.constructor;
      try {
        delete p.constructor;
        return p.constructor;
      }
      finally {
        p.constructor = ownValue;
      }
    }
    return p.constructor;
  }
};

ss.getTypeFullName = function (type) {
  return type.__typeName || type.name || (type.toString().match(/^\s*function\s*([^\s(]+)/) || [])[1] || 'Object';
};

ss._makeQName = function (name, asm) {
  return name + (asm ? ', ' + asm.name : '');
};

ss.getTypeQName = function (type) {
  return ss._makeQName(ss.getTypeFullName(type), type.__assembly);
};

ss.getTypeName = function (type) {
  var fullName = ss.getTypeFullName(type);
  var bIndex = fullName.indexOf('[');
  var nsIndex = fullName.lastIndexOf('.', bIndex >= 0 ? bIndex : fullName.length);
  return nsIndex > 0 ? fullName.substr(nsIndex + 1) : fullName;
};

ss.getTypeNamespace = function (type) {
  var fullName = ss.getTypeFullName(type);
  var bIndex = fullName.indexOf('[');
  var nsIndex = fullName.lastIndexOf('.', bIndex >= 0 ? bIndex : fullName.length);
  return nsIndex > 0 ? fullName.substr(0, nsIndex) : '';
};

ss.getTypeAssembly = function (type) {
  if (ss.contains([Date, Number, Boolean, String, Function, Array], type))
    return ss;
  else
    return type.__assembly || null;
};

ss._getAssemblyType = function (asm, name) {
  if (asm.__types) {
    return asm.__types[name] || null;
  }
  else {
    var a = name.split('.');
    for (var i = 0; i < a.length; i++) {
      asm = asm[a[i]];
      if (!ss.isValue(asm))
        return null;
    }
    if (typeof asm !== 'function')
      return null;
    return asm;
  }
};

ss.getAssemblyTypes = function (asm) {
  var result = [];
  if (asm.__types) {
    for (var t in asm.__types) {
      if (asm.__types.hasOwnProperty(t))
        result.push(asm.__types[t]);
    }
  }
  else {
    var traverse = function (s, n) {
      for (var c in s) {
        if (s.hasOwnProperty(c))
          traverse(s[c], c);
      }
      if (typeof (s) === 'function' && ss.isUpper(n.charCodeAt(0)))
        result.push(s);
    };
    traverse(asm, '');
  }
  return result;
};

ss.createAssemblyInstance = function (asm, typeName) {
  var t = ss.getType(typeName, asm);
  return t ? ss.createInstance(t) : null;
};

ss.getInterfaces = function (type) {
  if (type.__interfaces)
    return type.__interfaces;
  else if (type === Date || type === Number)
    return [ss_IEquatable, ss_IComparable, ss_IFormattable];
  else if (type === Boolean || type === String)
    return [ss_IEquatable, ss_IComparable];
  else if (type === Array || ss.isTypedArrayType(type))
    return [ss_IEnumerable, ss_ICollection, ss_IList, ss_IReadOnlyCollection, ss_IReadOnlyList];
  else
    return [];
};

ss.isInstanceOfType = function (instance, type) {
  if (ss.isNullOrUndefined(instance))
    return false;

  if (typeof (type.isInstanceOfType) === 'function')
    return type.isInstanceOfType(instance);

  return ss.isAssignableFrom(type, ss.getInstanceType(instance));
};

ss.isAssignableFrom = function (target, type) {
  return target === type || (typeof (target.isAssignableFrom) === 'function' && target.isAssignableFrom(type)) || type.prototype instanceof target;
};

ss.isClass = function (type) {
  return type.__class === true || type === Array || type === Function || type === RegExp || type === String || type === Error || type === Object;
};

ss.isEnum = function (type) {
  return !!type.__enum;
};

ss.isFlags = function (type) {
  return type.__metadata && type.__metadata.enumFlags || false;
};

ss.isInterface = function (type) {
  return !!type.__interface;
};

ss.safeCast = function (instance, type) {
  if (type === true)
    return instance;
  else if (type === false)
    return null;
  else
    return ss.isInstanceOfType(instance, type) ? instance : null;
};

ss.cast = function (instance, type) {
  if (instance === null || typeof (instance) === 'undefined')
    return instance;
  else if (type === true || (type !== false && ss.isInstanceOfType(instance, type)))
    return instance;
  throw new ss_InvalidCastException('Cannot cast object to type ' + ss.getTypeFullName(type));
};

ss.getInstanceType = function (instance) {
  if (!ss.isValue(instance))
    throw new ss_NullReferenceException('Cannot get type of null');

  // NOTE: We have to catch exceptions because the constructor
  //       cannot be looked up on native COM objects
  try {
    return instance.constructor;
  }
  catch (ex) {
    return Object;
  }
};

ss._getType = function (typeName, asm, re) {
  var outer = !re;
  re = re || /[[,\]]/g;
  var last = re.lastIndex, m = re.exec(typeName), tname, targs = [];
  var t;
  if (m) {
    tname = typeName.substring(last, m.index);
    switch (m[0]) {
      case '[':
        if (typeName[m.index + 1] !== '[')
          return null;
        for (; ;) {
          re.exec(typeName);
          t = ss._getType(typeName, global, re);
          if (!t)
            return null;
          targs.push(t);
          m = re.exec(typeName);
          if (m[0] === ']')
            break;
          else if (m[0] !== ',')
            return null;
        }
        m = re.exec(typeName);
        if (m && m[0] === ',') {
          re.exec(typeName);
          if (!(asm = ss.__assemblies[(re.lastIndex > 0 ? typeName.substring(m.index + 1, re.lastIndex - 1) : typeName.substring(m.index + 1)).trim()]))
            return null;
        }
        break;

      case ']':
        break;

      case ',':
        re.exec(typeName);
        if (!(asm = ss.__assemblies[(re.lastIndex > 0 ? typeName.substring(m.index + 1, re.lastIndex - 1) : typeName.substring(m.index + 1)).trim()]))
          return null;
        break;
    }
  }
  else {
    tname = typeName.substring(last);
  }

  if (outer && re.lastIndex)
    return null;

  t = ss._getAssemblyType(asm, tname.trim());
  return targs.length ? ss.makeGenericType(t, targs) : t;
}

ss.getType = function (typeName, asm) {
  return typeName ? ss._getType(typeName, asm || global) : null;
};

ss.getDefaultValue = function (type) {
  if (typeof (type.getDefaultValue) === 'function')
    return type.getDefaultValue();
  else if (type === Boolean)
    return false;
  else if (type === Date)
    return new Date(0);
  else if (type === Number)
    return 0;
  return null;
};

ss.createInstance = function (type) {
  if (typeof (type.createInstance) === 'function')
    return type.createInstance();
  else if (type === Boolean)
    return false;
  else if (type === Date)
    return new Date(0);
  else if (type === Number)
    return 0;
  else if (type === String)
    return '';
  else
    return new type();
};

ss.applyConstructor = function (constructor, args) {
  var f = function () {
    constructor.apply(this, args);
  };
  f.prototype = constructor.prototype;
  return new f();
};

ss.getAttributes = function (type, attrType, inherit) {
  var result = [];
  var a, i, t;
  if (inherit) {
    var b = ss.getBaseType(type);
    if (b) {
      a = ss.getAttributes(b, attrType, true);
      for (i = 0; i < a.length; i++) {
        t = ss.getInstanceType(a[i]);
        if (!t.__metadata || !t.__metadata.attrNoInherit)
          result.push(a[i]);
      }
    }
  }
  if (type.__metadata && type.__metadata.attr) {
    for (i = 0; i < type.__metadata.attr.length; i++) {
      a = type.__metadata.attr[i];
      if (attrType == null || ss.isInstanceOfType(a, attrType)) {
        t = ss.getInstanceType(a);
        if (!t.__metadata || !t.__metadata.attrAllowMultiple) {
          for (var j = result.length - 1; j >= 0; j--) {
            if (ss.isInstanceOfType(result[j], t))
              result.splice(j, 1);
          }
        }
        result.push(a);
      }
    }
  }
  return result;
};

ss.getMembers = function (type, memberTypes, bindingAttr, name, params) {
  var result = [];
  if ((bindingAttr & 72) === 72 || (bindingAttr & 6) === 4) {
    var b = ss.getBaseType(type);
    if (b)
      result = ss.getMembers(b, memberTypes & ~1, bindingAttr & (bindingAttr & 64 ? 255 : 247) & (bindingAttr & 2 ? 251 : 255), name, params);
  }

  var f = function (m) {
    if ((memberTypes & m.type) && (((bindingAttr & 4) && !m.isStatic) || ((bindingAttr & 8) && m.isStatic)) && (!name || m.name === name)) {
      if (params) {
        if ((m.params || []).length !== params.length)
          return;
        for (var i = 0; i < params.length; i++) {
          if (params[i] !== m.params[i])
            return;
        }
      }
      result.push(m);
    }
  };

  var i;
  if (type.__metadata && type.__metadata.members) {
    for (i = 0; i < type.__metadata.members.length; i++) {
      var m = type.__metadata.members[i];
      f(m);
      for (var j = 0; j < 4; j++) {
        var a = ['getter', 'setter', 'adder', 'remover'][j];
        if (m[a])
          f(m[a]);
      }
    }
  }

  if (bindingAttr & 256) {
    while (type) {
      var r = [];
      for (i = 0; i < result.length; i++) {
        if (result[i].typeDef === type)
          r.push(result[i]);
      }
      if (r.length > 1)
        throw new ss_AmbiguousMatchException('Ambiguous match');
      else if (r.length === 1)
        return r[0];
      type = ss.getBaseType(type);
    }
    return null;
  }

  return result;
};

ss.midel = function (mi, target, typeArguments) {
  if (mi.isStatic && !!target)
    throw new ss_ArgumentException('Cannot specify target for static method');
  else if (!mi.isStatic && !target)
    throw new ss_ArgumentException('Must specify target for instance method');

  var method;
  if (mi.fget) {
    method = function () { return (mi.isStatic ? mi.typeDef : this)[mi.fget]; };
  }
  else if (mi.fset) {
    method = function (v) { (mi.isStatic ? mi.typeDef : this)[mi.fset] = v; };
  }
  else {
    method = mi.def || (mi.isStatic || mi.sm ? mi.typeDef[mi.sname] : target[mi.sname]);

    if (mi.tpcount) {
      if (!typeArguments || typeArguments.length !== mi.tpcount)
        throw new ss_ArgumentException('Wrong number of type arguments');
      method = method.apply(null, typeArguments);
    }
    else {
      if (typeArguments && typeArguments.length)
        throw new ss_ArgumentException('Cannot specify type arguments for non-generic method');
    }
    if (mi.exp) {
      var m1 = method;
      method = function () { return m1.apply(this, Array.prototype.slice.call(arguments, 0, arguments.length - 1).concat(arguments[arguments.length - 1])); };
    }
    if (mi.sm) {
      var m2 = method;
      method = function () { return m2.apply(null, [this].concat(Array.prototype.slice.call(arguments))); };
    }
  }
  return ss.mkdel(target, method);
};

ss.invokeCI = function (ci, args) {
  if (ci.exp)
    args = args.slice(0, args.length - 1).concat(args[args.length - 1]);

  if (ci.def)
    return ci.def.apply(null, args);
  else if (ci.sm)
    return ci.typeDef[ci.sname].apply(null, args);
  else
    return ss.applyConstructor(ci.sname ? ci.typeDef[ci.sname] : ci.typeDef, args);
};

ss.fieldAccess = function (fi, obj) {
  if (fi.isStatic && !!obj)
    throw new ss_ArgumentException('Cannot specify target for static field');
  else if (!fi.isStatic && !obj)
    throw new ss_ArgumentException('Must specify target for instance field');
  obj = fi.isStatic ? fi.typeDef : obj;
  if (arguments.length === 3)
    obj[fi.sname] = arguments[2];

  return obj[fi.sname];
};

///////////////////////////////////////////////////////////////////////////////
// IFormattable

var ss_IFormattable = ss.IFormattable = ss.mkType(ss, 'ss.IFormattable');
ss.initInterface(ss_IFormattable, { format: null });

ss.format = function (obj, fmt) {
  if (typeof obj === 'number')
    return ss.formatNumber(obj, fmt);
  else if (ss.isDate(obj))
    return ss.formatDate(obj, fmt);
  else
    return obj.format(fmt);
};

///////////////////////////////////////////////////////////////////////////////
// IComparable

var ss_IComparable = ss.IComparable = ss.mkType(ss, 'ss.IComparable');
ss.initInterface(ss_IComparable, { compareTo: null });

///////////////////////////////////////////////////////////////////////////////
// IEquatable

var ss_IEquatable = ss.IEquatable = ss.mkType(ss, 'ss.IEquatable');
ss.initInterface(ss_IEquatable, { equalsT: null });

///////////////////////////////////////////////////////////////////////////////
// Number Extensions

ss.formatNumber = function (num, format) {
  if (ss.isNullOrUndefined(format) || (format.length === 0) || (format === 'i')) {
    return num.toString();
  }
  return ss.netFormatNumber(num, format, ss_CultureInfo.invariantCulture.numberFormat);
};

ss.localeFormatNumber = function (num, format) {
  if (ss.isNullOrUndefined(format) || (format.length === 0) || (format === 'i')) {
    return num.toLocaleString();
  }
  return ss.netFormatNumber(num, format, ss_CultureInfo.currentCulture.numberFormat);
};

ss._commaFormatNumber = function (number, groups, decimal, comma) {
  var decimalPart = null;
  var decimalIndex = number.indexOf(decimal);
  if (decimalIndex > 0) {
    decimalPart = number.substr(decimalIndex);
    number = number.substr(0, decimalIndex);
  }

  var negative = ss.startsWithString(number, '-');
  if (negative) {
    number = number.substr(1);
  }

  var groupIndex = 0;
  var groupSize = groups[groupIndex];
  if (number.length < groupSize) {
    return (negative ? '-' : '') + (decimalPart ? number + decimalPart : number);
  }

  var index = number.length;
  var s = '';
  var done = false;
  while (!done) {
    var length = groupSize;
    var startIndex = index - length;
    if (startIndex < 0) {
      groupSize += startIndex;
      length += startIndex;
      startIndex = 0;
      done = true;
    }
    if (!length) {
      break;
    }

    var part = number.substr(startIndex, length);
    if (s.length) {
      s = part + comma + s;
    }
    else {
      s = part;
    }
    index -= length;

    if (groupIndex < groups.length - 1) {
      groupIndex++;
      groupSize = groups[groupIndex];
    }
  }

  if (negative) {
    s = '-' + s;
  }
  return decimalPart ? s + decimalPart : s;
};

ss.netFormatNumber = function (num, format, numberFormat) {
  var nf = (numberFormat && numberFormat.getFormat(ss_NumberFormatInfo)) || ss_CultureInfo.currentCulture.numberFormat;

  var s = '';
  var precision = -1;

  if (format.length > 1) {
    precision = parseInt(format.substr(1), 10);
  }

  var fs = format.charAt(0);
  var index;
  switch (fs) {
    case 'd': case 'D':
      s = parseInt(Math.abs(num)).toString();
      if (precision !== -1) {
        s = ss.padLeftString(s, precision, 0x30);
      }
      if (num < 0) {
        s = '-' + s;
      }
      break;
    case 'x': case 'X':
      s = parseInt(Math.abs(num)).toString(16);
      if (fs === 'X') {
        s = s.toUpperCase();
      }
      if (precision !== -1) {
        s = ss.padLeftString(s, precision, 0x30);
      }
      break;
    case 'e': case 'E':
      if (precision === -1) {
        s = num.toExponential();
      }
      else {
        s = num.toExponential(precision);
      }
      if (fs === 'E') {
        s = s.toUpperCase();
      }
      break;
    case 'f': case 'F':
    case 'n': case 'N':
      if (precision === -1) {
        precision = nf.numberDecimalDigits;
      }
      s = num.toFixed(precision).toString();
      if (precision && (nf.numberDecimalSeparator !== '.')) {
        index = s.indexOf('.');
        s = s.substr(0, index) + nf.numberDecimalSeparator + s.substr(index + 1);
      }
      if ((fs === 'n') || (fs === 'N')) {
        s = ss._commaFormatNumber(s, nf.numberGroupSizes, nf.numberDecimalSeparator, nf.numberGroupSeparator);
      }
      break;
    case 'c': case 'C':
      if (precision === -1) {
        precision = nf.currencyDecimalDigits;
      }
      s = Math.abs(num).toFixed(precision).toString();
      if (precision && (nf.currencyDecimalSeparator !== '.')) {
        index = s.indexOf('.');
        s = s.substr(0, index) + nf.currencyDecimalSeparator + s.substr(index + 1);
      }
      s = ss._commaFormatNumber(s, nf.currencyGroupSizes, nf.currencyDecimalSeparator, nf.currencyGroupSeparator);
      if (num < 0) {
        s = ss.formatString(nf.currencyNegativePattern, s);
      }
      else {
        s = ss.formatString(nf.currencyPositivePattern, s);
      }
      break;
    case 'p': case 'P':
      if (precision === -1) {
        precision = nf.percentDecimalDigits;
      }
      s = (Math.abs(num) * 100.0).toFixed(precision).toString();
      if (precision && (nf.percentDecimalSeparator !== '.')) {
        index = s.indexOf('.');
        s = s.substr(0, index) + nf.percentDecimalSeparator + s.substr(index + 1);
      }
      s = ss._commaFormatNumber(s, nf.percentGroupSizes, nf.percentDecimalSeparator, nf.percentGroupSeparator);
      if (num < 0) {
        s = ss.formatString(nf.percentNegativePattern, s);
      }
      else {
        s = ss.formatString(nf.percentPositivePattern, s);
      }
      break;
  }

  return s;
};

///////////////////////////////////////////////////////////////////////////////
// String Extensions
ss.netSplit = function (s, strings, limit, options) {
  var re = new RegExp(strings.map(ss.regexpEscape).join('|'), 'g'), res = [], m, i;
  for (i = 0; ; i = re.lastIndex) {
    if (m = re.exec(s)) {
      if (options !== 1 || m.index > i) {
        if (res.length === limit - 1) {
          res.push(s.substr(i));
          return res;
        }
        else
          res.push(s.substring(i, m.index));
      }
    }
    else {
      if (options !== 1 || i !== s.length)
        res.push(s.substr(i));
      return res;
    }
  }
};

ss.compareStrings = function (s1, s2, ignoreCase) {
  if (!ss.isValue(s1))
    return ss.isValue(s2) ? -1 : 0;
  if (!ss.isValue(s2))
    return 1;

  if (ignoreCase) {
    if (s1) {
      s1 = s1.toUpperCase();
    }
    if (s2) {
      s2 = s2.toUpperCase();
    }
  }
  s1 = s1 || '';
  s2 = s2 || '';

  if (s1 === s2) {
    return 0;
  }
  if (s1 < s2) {
    return -1;
  }
  return 1;
};

ss.endsWithString = function (s, suffix) {
  if (!suffix.length) {
    return true;
  }
  if (suffix.length > s.length) {
    return false;
  }
  return s.substr(s.length - suffix.length) === suffix;
};

ss._formatString = function (format, values, useLocale) {
  if (!ss._formatRE) {
    ss._formatRE = /\{\{|\}\}|\{[^\}\{]+\}/g;
  }

  return format.replace(ss._formatRE,
    function (m) {
      if (m === '{{' || m === '}}')
        return m.charAt(0);
      var index = parseInt(m.substr(1), 10);
      var value = values[index + 1];
      if (ss.isNullOrUndefined(value)) {
        return '';
      }
      if (ss.isInstanceOfType(value, ss_IFormattable)) {
        var formatSpec = null;
        var formatIndex = m.indexOf(':');
        if (formatIndex > 0) {
          formatSpec = m.substring(formatIndex + 1, m.length - 1);
        }
        return ss.format(value, formatSpec);
      }
      else {
        return useLocale ? value.toLocaleString() : value.toString();
      }
    });
};

ss.formatString = function (format) {
  return ss._formatString(format, arguments, /* useLocale */ false);
};

ss.stringFromChar = function (ch, count) {
  var s = ch;
  for (var i = 1; i < count; i++) {
    s += ch;
  }
  return s;
};

ss.htmlDecode = function (s) {
  return s.replace(/&([^;]+);/g, function (_, e) {
    if (e[0] === '#')
      return String.fromCharCode(parseInt(e.substr(1), 10));
    switch (e) {
      case 'quot': return '"';
      case 'apos': return "'";
      case 'amp': return '&';
      case 'lt': return '<';
      case 'gt': return '>';
      default: return '&' + e + ';';
    }
  });
};

ss.htmlEncode = function (s) {
  return s.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
};

ss.jsEncode = function (s, q) {
  s = s.replace(/\\/g, '\\\\').replace(/'/g, "\\'").replace(/"/g, '\\"');
  return q ? '"' + s + '"' : s;
};

ss.indexOfAnyString = function (s, chars, startIndex, count) {
  var length = s.length;
  if (!length) {
    return -1;
  }

  chars = String.fromCharCode.apply(null, chars);
  startIndex = startIndex || 0;
  count = count || length;

  var endIndex = startIndex + count - 1;
  if (endIndex >= length) {
    endIndex = length - 1;
  }

  for (var i = startIndex; i <= endIndex; i++) {
    if (chars.indexOf(s.charAt(i)) >= 0) {
      return i;
    }
  }
  return -1;
};

ss.insertString = function (s, index, value) {
  if (!value) {
    return s;
  }
  if (!index) {
    return value + s;
  }
  var s1 = s.substr(0, index);
  var s2 = s.substr(index);
  return s1 + value + s2;
};

ss.isNullOrEmptyString = function (s) {
  return !s || !s.length;
};

ss.lastIndexOfAnyString = function (s, chars, startIndex, count) {
  var length = s.length;
  if (!length) {
    return -1;
  }

  chars = String.fromCharCode.apply(null, chars);
  startIndex = startIndex || length - 1;
  count = count || length;

  var endIndex = startIndex - count + 1;
  if (endIndex < 0) {
    endIndex = 0;
  }

  for (var i = startIndex; i >= endIndex; i--) {
    if (chars.indexOf(s.charAt(i)) >= 0) {
      return i;
    }
  }
  return -1;
};

ss.localeFormatString = function (format) {
  return ss._formatString(format, arguments, /* useLocale */ true);
};

ss.padLeftString = function (s, totalWidth, ch) {
  if (s.length < totalWidth) {
    ch = String.fromCharCode(ch || 0x20);
    return ss.stringFromChar(ch, totalWidth - s.length) + s;
  }
  return s;
};

ss.padRightString = function (s, totalWidth, ch) {
  if (s.length < totalWidth) {
    ch = String.fromCharCode(ch || 0x20);
    return s + ss.stringFromChar(ch, totalWidth - s.length);
  }
  return s;
};

ss.removeString = function (s, index, count) {
  if (!count || ((index + count) > this.length)) {
    return s.substr(0, index);
  }
  return s.substr(0, index) + s.substr(index + count);
};

ss.replaceAllString = function (s, oldValue, newValue) {
  newValue = newValue || '';
  return s.split(oldValue).join(newValue);
};

ss.startsWithString = function (s, prefix) {
  if (!prefix.length) {
    return true;
  }
  if (prefix.length > s.length) {
    return false;
  }
  return s.substr(0, prefix.length) === prefix;
};

if (!String.prototype.trim) {
  String.prototype.trim = function () {
    return ss.trimStartString(ss.trimEndString(this));
  };
}

ss.trimEndString = function (s, chars) {
  return s.replace(chars ? new RegExp('[' + String.fromCharCode.apply(null, chars) + ']+$') : /\s*$/, '');
};

ss.trimStartString = function (s, chars) {
  return s.replace(chars ? new RegExp('^[' + String.fromCharCode.apply(null, chars) + ']+') : /^\s*/, '');
};

ss.trimString = function (s, chars) {
  return ss.trimStartString(ss.trimEndString(s, chars), chars);
};

ss.lastIndexOfString = function (s, search, startIndex, count) {
  var index = s.lastIndexOf(search, startIndex);
  return (index < (startIndex - count + 1)) ? -1 : index;
};

ss.indexOfString = function (s, search, startIndex, count) {
  var index = s.indexOf(search, startIndex);
  return ((index + search.length) <= (startIndex + count)) ? index : -1;
};

///////////////////////////////////////////////////////////////////////////////
// Math Extensions

ss.divRem = function (a, b, result) {
  var remainder = a % b;
  result.$ = remainder;
  return (a - remainder) / b;
};

ss.round = function (n, d, rounding) {
  var m = Math.pow(10, d || 0);
  n *= m;
  var sign = (n > 0) | -(n < 0);
  if (n % 1 === 0.5 * sign) {
    var f = Math.floor(n);
    return (f + (rounding ? (sign > 0) : (f % 2 * sign))) / m;
  }

  return Math.round(n) / m;
};

///////////////////////////////////////////////////////////////////////////////
// IFormatProvider

var ss_IFormatProvider = ss.IFormatProvider = ss.mkType(ss, 'ss.IFormatProvider');
ss.initInterface(ss_IFormatProvider, { getFormat: null });

///////////////////////////////////////////////////////////////////////////////
// NumberFormatInfo

var ss_NumberFormatInfo = ss.NumberFormatInfo = ss.mkType(ss, 'ss.NumberFormatInfo',
  function () {
  },
  {
    getFormat: function (type) {
      return (type === ss_NumberFormatInfo) ? this : null;
    }
  }
);

ss.initClass(ss_NumberFormatInfo, null, [ss_IFormatProvider]);

ss_NumberFormatInfo.invariantInfo = new ss_NumberFormatInfo();
ss.shallowCopy({
  naNSymbol: 'NaN',
  negativeSign: '-',
  positiveSign: '+',
  negativeInfinitySymbol: '-Infinity',
  positiveInfinitySymbol: 'Infinity',

  percentSymbol: '%',
  percentGroupSizes: [3],
  percentDecimalDigits: 2,
  percentDecimalSeparator: '.',
  percentGroupSeparator: ',',
  percentPositivePattern: 0,
  percentNegativePattern: 0,

  currencySymbol: '$',
  currencyGroupSizes: [3],
  currencyDecimalDigits: 2,
  currencyDecimalSeparator: '.',
  currencyGroupSeparator: ',',
  currencyNegativePattern: 0,
  currencyPositivePattern: 0,

  numberGroupSizes: [3],
  numberDecimalDigits: 2,
  numberDecimalSeparator: '.',
  numberGroupSeparator: ','
}, ss_NumberFormatInfo.invariantInfo);

///////////////////////////////////////////////////////////////////////////////
// DateTimeFormatInfo

var ss_DateTimeFormatInfo = ss.DateTimeFormatInfo = ss.mkType(ss, 'ss.DateTimeFormatInfo',
  function () {
  },
  {
    getFormat: function (type) {
      return type === ss_DateTimeFormatInfo ? this : null;
    }
  }
);

ss.initClass(ss_DateTimeFormatInfo, null, [ss_IFormatProvider]);

ss_DateTimeFormatInfo.invariantInfo = new ss_DateTimeFormatInfo();
ss.shallowCopy({
  amDesignator: 'AM',
  pmDesignator: 'PM',

  dateSeparator: '/',
  timeSeparator: ':',

  gmtDateTimePattern: 'ddd, dd MMM yyyy HH:mm:ss \'GMT\'',
  universalDateTimePattern: 'yyyy-MM-dd HH:mm:ssZ',
  sortableDateTimePattern: 'yyyy-MM-ddTHH:mm:ss',
  dateTimePattern: 'dddd, MMMM dd, yyyy h:mm:ss tt',

  longDatePattern: 'dddd, MMMM dd, yyyy',
  shortDatePattern: 'M/d/yyyy',

  longTimePattern: 'h:mm:ss tt',
  shortTimePattern: 'h:mm tt',

  firstDayOfWeek: 0,
  dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
  shortDayNames: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
  minimizedDayNames: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],

  monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December', ''],
  shortMonthNames: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec', '']
}, ss_DateTimeFormatInfo.invariantInfo);

  //#include "Stopwatch.js"

///////////////////////////////////////////////////////////////////////////////
// Array Extensions

ss._flatIndex = function (arr, indices) {
  if (indices.length !== (arr._sizes ? arr._sizes.length : 1))
    throw new ss_ArgumentException('Invalid number of indices');

  if (indices[0] < 0 || indices[0] >= (arr._sizes ? arr._sizes[0] : arr.length))
    throw new ss_ArgumentException('Index 0 out of range');

  var idx = indices[0];
  if (arr._sizes) {
    for (var i = 1; i < arr._sizes.length; i++) {
      if (indices[i] < 0 || indices[i] >= arr._sizes[i])
        throw new ss_ArgumentException('Index ' + i + ' out of range');
      idx = idx * arr._sizes[i] + indices[i];
    }
  }
  return idx;
};

ss.arrayGet2 = function (arr, indices) {
  var idx = ss._flatIndex(arr, indices);
  var r = arr[idx];
  return typeof r !== 'undefined' ? r : arr._defvalue;
};

ss.arrayGet = function (arr) {
  return ss.arrayGet2(arr, Array.prototype.slice.call(arguments, 1));
};

ss.arraySet2 = function (arr, value, indices) {
  var idx = ss._flatIndex(arr, indices);
  arr[idx] = value;
};

ss.arraySet = function () {
  return ss.arraySet2(arguments[0], arguments[arguments.length - 1], Array.prototype.slice.call(arguments, 1, arguments.length - 1));
};

ss.arrayRank = function (arr) {
  return arr._sizes ? arr._sizes.length : 1;
};

ss.arrayLength = function (arr, dimension) {
  if (dimension >= (arr._sizes ? arr._sizes.length : 1))
    throw new ss_ArgumentException('Invalid dimension');
  return arr._sizes ? arr._sizes[dimension] : arr.length;
};

ss.arrayExtract = function (arr, start, count) {
  if (!ss.isValue(count)) {
    return arr.slice(start);
  }
  return arr.slice(start, start + count);
};

ss.arrayAddRange = function (arr, items) {
  if (items instanceof Array) {
    arr.push.apply(arr, items);
  }
  else {
    var e = ss.getEnumerator(items);
    try {
      while (e.moveNext()) {
        ss.add(arr, e.current());
      }
    }
    finally {
      if (ss.isInstanceOfType(e, ss_IDisposable)) {
        ss.cast(e, ss_IDisposable).dispose();
      }
    }
  }
};

ss.arrayClone = function (arr) {
  if (arr.length === 1) {
    return [arr[0]];
  }
  else {
    return Array.apply(null, arr);
  }
};

ss.arrayPeekFront = function (arr) {
  if (arr.length)
    return arr[0];
  throw new ss_InvalidOperationException('Array is empty');
};

ss.arrayPeekBack = function (arr) {
  if (arr.length)
    return arr[arr.length - 1];
  throw new ss_InvalidOperationException('Array is empty');
};

ss.indexOfArray = function (arr, item, startIndex) {
  startIndex = startIndex || 0;
  for (var i = startIndex; i < arr.length; i++) {
    if (ss.staticEquals(arr[i], item)) {
      return i;
    }
  }
  return -1;
}

ss.arrayInsertRange = function (arr, index, items) {
  if (items instanceof Array) {
    if (index === 0) {
      arr.unshift.apply(arr, items);
    }
    else {
      for (var i = 0; i < items.length; i++) {
        arr.splice(index + i, 0, items[i]);
      }
    }
  }
  else {
    var e = ss.getEnumerator(items);
    try {
      while (e.moveNext()) {
        arr.insert(index, e.current());
        index++;
      }
    }
    finally {
      if (ss.isInstanceOfType(e, ss_IDisposable)) {
        ss.cast(e, ss_IDisposable).dispose();
      }
    }
  }
};

if (!Array.prototype.map) {
  Array.prototype.map = function (callback, instance) {
    var length = this.length;
    var mapped = new Array(length);
    for (var i = 0; i < length; i++) {
      if (i in this) {
        mapped[i] = callback.call(instance, this[i], i, this);
      }
    }
    return mapped;
  };
}

ss.arrayRemoveRange = function (arr, index, count) {
  arr.splice(index, count);
};

if (!Array.prototype.some) {
  Array.prototype.some = function (callback, instance) {
    var length = this.length;
    for (var i = 0; i < length; i++) {
      if (i in this && callback.call(instance, this[i], i, this)) {
        return true;
      }
    }
    return false;
  };
}

// Production steps of ECMA-262, Edition 5, 15.4.4.18
// Reference: http://es5.github.io/#x15.4.4.18
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/forEach
if (!Array.prototype.forEach) {
  Array.prototype.forEach = function (callback, thisArg) {
    var T, k;

    if (this == null) {
      throw new TypeError(' this is null or not defined');
    }

    // 1. Let O be the result of calling ToObject passing the |this| value as the argument.
    var O = Object(this);

    // 2. Let lenValue be the result of calling the Get internal method of O with the argument "length".
    // 3. Let len be ToUint32(lenValue).
    var len = O.length >>> 0;

    // 4. If IsCallable(callback) is false, throw a TypeError exception.
    // See: http://es5.github.com/#x9.11
    if (typeof callback !== 'function') {
      throw new TypeError(callback + ' is not a function');
    }

    // 5. If thisArg was supplied, let T be thisArg; else let T be undefined.
    if (arguments.length > 1) {
      T = thisArg;
    }

    // 6. Let k be 0
    k = 0;

    // 7. Repeat, while k < len
    while (k < len) {
      var kValue;

      // a. Let Pk be ToString(k).
      //   This is implicit for LHS operands of the in operator
      // b. Let kPresent be the result of calling the HasProperty internal method of O with argument Pk.
      //   This step can be combined with c
      // c. If kPresent is true, then
      if (k in O) {
        // i. Let kValue be the result of calling the Get internal method of O with argument Pk.
        kValue = O[k];

        // ii. Call the Call internal method of callback with T as the this value and
        // argument list containing kValue, k, and O.
        callback.call(T, kValue, k, O);
      }
      // d. Increase k by 1.
      k++;
    }
    // 8. return undefined
  };
}

// Production steps of ECMA-262, Edition 5
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/filter
if (!Array.prototype.filter) {
  Array.prototype.filter = function (fun/*, thisArg*/) {
    if (this === void 0 || this === null) {
      throw new TypeError();
    }

    var t = Object(this);
    var len = t.length >>> 0;
    if (typeof fun !== 'function') {
      throw new TypeError();
    }

    var res = [];
    var thisArg = arguments.length >= 2 ? arguments[1] : void 0;
    for (var i = 0; i < len; i++) {
      if (i in t) {
        var val = t[i];

        // NOTE: Technically this should Object.defineProperty at
        //       the next index, as push can be affected by
        //       properties on Object.prototype and Array.prototype.
        //       But that method's new, and collisions should be
        //       rare, so use the more-compatible alternative.
        if (fun.call(thisArg, val, i, t)) {
          res.push(val);
        }
      }
    }

    return res;
  };
}

ss.arrayFromEnumerable = function (enm) {
  if (!ss.isValue(enm))
    return null;

  var e = ss.getEnumerator(enm), r = [];
  try {
    while (e.moveNext())
      r.push(e.current());
  }
  finally {
    e.dispose();
  }
  return r;
};

ss.multidimArray = function (defvalue) {
  var arr = [];
  arr._defvalue = defvalue;
  arr._sizes = [arguments[1]];
  var length = arguments[1];
  for (var i = 2; i < arguments.length; i++) {
    length *= arguments[i];
    arr._sizes[i - 1] = arguments[i];
  }
  arr.length = length;
  return arr;
};

ss.repeat = function (value, count) {
  var result = [];
  for (var i = 0; i < count; i++)
    result.push(value);
  return result;
};

ss.arrayFill = function (dst, val, index, count) {
  if (index < 0 || count < 0 || (index + count) > dst.length)
    throw new ss_ArgumentException();
  if (Array.prototype.fill) {
    dst.fill(val, index, index + count);
  }
  else {
    while (--count >= 0)
      dst[index + count] = val;
  }
};

ss.arrayCopy = function (src, spos, dst, dpos, len) {
  if (spos < 0 || dpos < 0 || len < 0)
    throw new ss_ArgumentOutOfRangeException();

  if (len > (src.length - spos) || len > (dst.length - dpos))
    throw new ss_ArgumentException();

  if (spos < dpos && src === dst) {
    while (--len >= 0)
      dst[dpos + len] = src[spos + len];
  }
  else {
    for (var i = 0; i < len; i++)
      dst[dpos + i] = src[spos + i];
  }
}

///////////////////////////////////////////////////////////////////////////////
// Date Extensions

ss.utcNow = function () {
  var d = new Date();
  return new Date(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate(), d.getUTCHours(), d.getUTCMinutes(), d.getUTCSeconds(), d.getUTCMilliseconds());
};

ss.toUTC = function (d) {
  return new Date(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate(), d.getUTCHours(), d.getUTCMinutes(), d.getUTCSeconds(), d.getUTCMilliseconds());
};

ss.fromUTC = function (d) {
  return new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate(), d.getHours(), d.getMinutes(), d.getSeconds(), d.getMilliseconds()));
};

ss.today = function () {
  var d = new Date();
  return new Date(d.getFullYear(), d.getMonth(), d.getDate());
}

ss.formatDate = function (date, format) {
  if (ss.isNullOrUndefined(format) || (format.length === 0) || (format === 'i')) {
    return date.toString();
  }
  if (format === 'id') {
    return date.toDateString();
  }
  if (format === 'it') {
    return date.toTimeString();
  }

  return ss._netFormatDate(date, format, false);
};

ss.localeFormatDate = function (date, format) {
  if (ss.isNullOrUndefined(format) || (format.length === 0) || (format === 'i')) {
    return date.toLocaleString();
  }
  if (format === 'id') {
    return date.toLocaleDateString();
  }
  if (format === 'it') {
    return date.toLocaleTimeString();
  }

  return ss._netFormatDate(date, format, true);
};

ss._netFormatDate = function (dt, format, useLocale) {
  var dtf = useLocale ? ss_CultureInfo.currentCulture.dateTimeFormat : ss_CultureInfo.invariantCulture.dateTimeFormat;

  if (format.length === 1) {
    switch (format) {
      case 'f': format = dtf.longDatePattern + ' ' + dtf.shortTimePattern; break;
      case 'F': format = dtf.dateTimePattern; break;

      case 'd': format = dtf.shortDatePattern; break;
      case 'D': format = dtf.longDatePattern; break;

      case 't': format = dtf.shortTimePattern; break;
      case 'T': format = dtf.longTimePattern; break;

      case 'g': format = dtf.shortDatePattern + ' ' + dtf.shortTimePattern; break;
      case 'G': format = dtf.shortDatePattern + ' ' + dtf.longTimePattern; break;

      case 'R': case 'r':
        dtf = ss_CultureInfo.InvariantCulture.dateTimeFormat;
        format = dtf.gmtDateTimePattern;
        break;
      case 'u': format = dtf.universalDateTimePattern; break;
      case 'U':
        format = dtf.dateTimePattern;
        dt = new Date(dt.getUTCFullYear(), dt.getUTCMonth(), dt.getUTCDate(),
          dt.getUTCHours(), dt.getUTCMinutes(), dt.getUTCSeconds(), dt.getUTCMilliseconds());
        break;

      case 's': format = dtf.sortableDateTimePattern; break;
    }
  }

  if (format.charAt(0) === '%') {
    format = format.substr(1);
  }

  if (!Date._formatRE) {
    Date._formatRE = /'.*?[^\\]'|dddd|ddd|dd|d|MMMM|MMM|MM|M|yyyy|yy|y|hh|h|HH|H|mm|m|ss|s|tt|t|fff|ff|f|zzz|zz|z/g;
  }

  var re = Date._formatRE;
  var sb = new ss_StringBuilder();

  re.lastIndex = 0;
  while (true) {
    var index = re.lastIndex;
    var match = re.exec(format);

    sb.append(format.slice(index, match ? match.index : format.length));
    if (!match) {
      break;
    }

    var fs = match[0];
    var part = fs;
    switch (fs) {
      case 'dddd':
        part = dtf.dayNames[dt.getDay()];
        break;
      case 'ddd':
        part = dtf.shortDayNames[dt.getDay()];
        break;
      case 'dd':
        part = ss.padLeftString(dt.getDate().toString(), 2, 0x30);
        break;
      case 'd':
        part = dt.getDate();
        break;
      case 'MMMM':
        part = dtf.monthNames[dt.getMonth()];
        break;
      case 'MMM':
        part = dtf.shortMonthNames[dt.getMonth()];
        break;
      case 'MM':
        part = ss.padLeftString((dt.getMonth() + 1).toString(), 2, 0x30);
        break;
      case 'M':
        part = (dt.getMonth() + 1);
        break;
      case 'yyyy':
        part = dt.getFullYear();
        break;
      case 'yy':
        part = ss.padLeftString((dt.getFullYear() % 100).toString(), 2, 0x30);
        break;
      case 'y':
        part = (dt.getFullYear() % 100);
        break;
      case 'h': case 'hh':
        part = dt.getHours() % 12;
        if (!part) {
          part = '12';
        }
        else if (fs === 'hh') {
          part = ss.padLeftString(part.toString(), 2, 0x30);
        }
        break;
      case 'HH':
        part = ss.padLeftString(dt.getHours().toString(), 2, 0x30);
        break;
      case 'H':
        part = dt.getHours();
        break;
      case 'mm':
        part = ss.padLeftString(dt.getMinutes().toString(), 2, 0x30);
        break;
      case 'm':
        part = dt.getMinutes();
        break;
      case 'ss':
        part = ss.padLeftString(dt.getSeconds().toString(), 2, 0x30);
        break;
      case 's':
        part = dt.getSeconds();
        break;
      case 't': case 'tt':
        part = (dt.getHours() < 12) ? dtf.amDesignator : dtf.pmDesignator;
        if (fs === 't') {
          part = part.charAt(0);
        }
        break;
      case 'fff':
        part = ss.padLeftString(dt.getMilliseconds().toString(), 3, 0x30);
        break;
      case 'ff':
        part = ss.padLeftString(dt.getMilliseconds().toString(), 3).substr(0, 2);
        break;
      case 'f':
        part = ss.padLeftString(dt.getMilliseconds().toString(), 3).charAt(0);
        break;
      case 'z':
        part = dt.getTimezoneOffset() / 60;
        part = ((part >= 0) ? '-' : '+') + Math.floor(Math.abs(part));
        break;
      case 'zz': case 'zzz':
        part = dt.getTimezoneOffset() / 60;
        part = ((part >= 0) ? '-' : '+') + Math.floor(ss.padLeftString(Math.abs(part)).toString(), 2, 0x30);
        if (fs === 'zzz') {
          part += dtf.timeSeparator + Math.abs(ss.padLeftString(dt.getTimezoneOffset() % 60).toString(), 2, 0x30);
        }
        break;
      default:
        if (part.charAt(0) === '\'') {
          part = part.substr(1, part.length - 2).replace(/\\'/g, '\'');
        }
        break;
    }
    sb.append(part);
  }

  return sb.toString();
};

ss._parseExactDate = function (val, format, provider, utc) {
  provider = (provider && provider.getFormat(ss_DateTimeFormatInfo)) || ss_CultureInfo.currentCulture.dateTimeFormat;
  var am = provider.amDesignator, pm = provider.pmDesignator;

  var _isInteger = function (val) {
    var digits = '1234567890';
    for (var i = 0; i < val.length; i++) {
      if (digits.indexOf(val.charAt(i)) === -1) {
        return false;
      }
    }
    return true;
  };

  var _getInt = function (str, i, minlength, maxlength) {
    for (var x = maxlength; x >= minlength; x--) {
      var token = str.substring(i, i + x);
      if (token.length < minlength) {
        return null;
      }
      if (_isInteger(token)) {
        return token;
      }
    }
    return null;
  };

  val = val + '';
  format = format + '';
  var iVal = 0;
  var iFormat = 0;
  var c = '';
  var token = '';

  var year = 0, month = 1, date = 1, hh = 0, mm = 0, _ss = 0, ampm = '';

  while (iFormat < format.length) {
    // Get next token from format string
    c = format.charAt(iFormat);
    token = '';
    while ((format.charAt(iFormat) === c) && (iFormat < format.length)) {
      token += format.charAt(iFormat++);
    }
    // Extract contents of value based on format token
    if (token === 'yyyy' || token === 'yy' || token === 'y') {
      if (token === 'yyyy')
        year = _getInt(val, iVal, 4, 4);
      if (token === 'yy')
        year = _getInt(val, iVal, 2, 2);
      if (token === 'y')
        year = _getInt(val, iVal, 2, 4);

      if (year == null)
        return null;

      iVal += year.length;
      if (year.length === 2) {
        if (year > 30) {
          year = 1900 + (year - 0);
        }
        else {
          year = 2000 + (year - 0);
        }
      }
    }
    else if (token === 'MM' || token === 'M') {
      month = _getInt(val, iVal, token.length, 2);
      if (month == null || (month < 1) || (month > 12))
        return null;
      iVal += month.length;
    }
    else if (token === 'dd' || token === 'd') {
      date = _getInt(val, iVal, token.length, 2);
      if (date == null || (date < 1) || (date > 31))
        return null;
      iVal += date.length;
    }
    else if (token === 'hh' || token === 'h') {
      hh = _getInt(val, iVal, token.length, 2);
      if (hh == null || (hh < 1) || (hh > 12))
        return null;
      iVal += hh.length;
    }
    else if (token === 'HH' || token === 'H') {
      hh = _getInt(val, iVal, token.length, 2);
      if (hh == null || (hh < 0) || (hh > 23))
        return null;
      iVal += hh.length;
    }
    else if (token === 'mm' || token === 'm') {
      mm = _getInt(val, iVal, token.length, 2);
      if (mm == null || (mm < 0) || (mm > 59))
        return null;
      iVal += mm.length;
    }
    else if (token === 'ss' || token === 's') {
      _ss = _getInt(val, iVal, token.length, 2);
      if (_ss == null || (_ss < 0) || (_ss > 59))
        return null;
      iVal += _ss.length;
    }
    else if (token === 't') {
      if (val.substring(iVal, iVal + 1).toLowerCase() === am.charAt(0).toLowerCase())
        ampm = am;
      else if (val.substring(iVal, iVal + 1).toLowerCase() === pm.charAt(0).toLowerCase())
        ampm = pm;
      else
        return null;
      iVal += 1;
    }
    else if (token === 'tt') {
      if (val.substring(iVal, iVal + 2).toLowerCase() === am.toLowerCase())
        ampm = am;
      else if (val.substring(iVal, iVal + 2).toLowerCase() === pm.toLowerCase())
        ampm = pm;
      else
        return null;
      iVal += 2;
    }
    else {
      if (val.substring(iVal, iVal + token.length) !== token)
        return null;
      else
        iVal += token.length;
    }
  }
  // If there are any trailing characters left in the value, it doesn't match
  if (iVal !== val.length)
    return null;

  // Is date valid for month?
  if (month === 2) {
    // Check for leap year
    if (((year % 4 === 0) && (year % 100 !== 0)) || (year % 400 === 0)) { // leap year
      if (date > 29)
        return null;
    }
    else if (date > 28)
      return null;
  }
  if ((month === 4) || (month === 6) || (month === 9) || (month === 11)) {
    if (date > 30) {
      return null;
    }
  }
  // Correct hours value
  if (hh < 12 && ampm === pm) {
    hh = hh - 0 + 12;
  }
  else if (hh > 11 && ampm === am) {
    hh -= 12;
  }

  if (utc)
    return new Date(Date.UTC(year, month - 1, date, hh, mm, _ss));
  else
    return new Date(year, month - 1, date, hh, mm, _ss);
};

ss.parseExactDate = function (val, format, provider) {
  return ss._parseExactDate(val, format, provider, false);
};

ss.parseExactDateUTC = function (val, format, provider) {
  return ss._parseExactDate(val, format, provider, true);
};

///////////////////////////////////////////////////////////////////////////////
// Function Extensions

ss._delegateContains = function (targets, object, method) {
  for (var i = 0; i < targets.length; i += 2) {
    if (targets[i] === object && targets[i + 1] === method) {
      return true;
    }
  }
  return false;
};

ss._mkdel = function (targets) {
  var delegate = function () {
    if (targets.length === 2) {
      return targets[1].apply(targets[0], arguments);
    }
    else {
      var clone = ss.arrayClone(targets);
      for (var i = 0; i < clone.length; i += 2) {
        if (ss._delegateContains(targets, clone[i], clone[i + 1])) {
          clone[i + 1].apply(clone[i], arguments);
        }
      }
      return null;
    }
  };
  delegate._targets = targets;

  return delegate;
};

ss.mkdel = function (object, method) {
  if (!object) {
    return method;
  }
  if (typeof method === 'string') {
    method = object[method];
  }
  return ss._mkdel([object, method]);
};

ss.delegateCombine = function (delegate1, delegate2) {
  if (!delegate1) {
    if (!delegate2._targets) {
      return ss.mkdel(null, delegate2);
    }
    return delegate2;
  }
  if (!delegate2) {
    if (!delegate1._targets) {
      return ss.mkdel(null, delegate1);
    }
    return delegate1;
  }

  var targets1 = delegate1._targets ? delegate1._targets : [null, delegate1];
  var targets2 = delegate2._targets ? delegate2._targets : [null, delegate2];

  return ss._mkdel(targets1.concat(targets2));
};

ss.delegateRemove = function (delegate1, delegate2) {
  if (!delegate1 || (delegate1 === delegate2)) {
    return null;
  }

  var targets = delegate1._targets;
  if (!delegate2 || !targets) {
    return delegate1;
  }

  var object = null;
  var method;
  if (delegate2._targets) {
    object = delegate2._targets[0];
    method = delegate2._targets[1];
  }
  else {
    method = delegate2;
  }

  for (var i = 0; i < targets.length; i += 2) {
    if ((targets[i] === object) && (targets[i + 1] === method)) {
      if (targets.length === 2) {
        return null;
      }
      var t = ss.arrayClone(targets);
      t.splice(i, 2);
      return ss._mkdel(t);
    }
  }

  return delegate1;
};

ss.delegateEquals = function (a, b) {
  if (a === b)
    return true;
  if (!a._targets && !b._targets)
    return false;
  var ta = a._targets || [null, a], tb = b._targets || [null, b];
  if (ta.length !== tb.length)
    return false;
  for (var i = 0; i < ta.length; i++) {
    if (ta[i] !== tb[i])
      return false;
  }
  return true;
};

ss.delegateClone = function (source) {
  return source._targets ? ss._mkdel(source._targets) : function () { return source.apply(this, arguments); };
};

ss.thisFix = function (source) {
  return function () {
    var x = [this];
    for (var i = 0; i < arguments.length; i++)
      x.push(arguments[i]);
    return source.apply(source, x);
  };
};

ss.getInvocationList = function (delegate) {
  if (!delegate._targets)
    return [delegate];
  var result = [];
  for (var i = 0; i < delegate._targets.length; i += 2)
    result.push(ss.mkdel(delegate._targets[i], delegate._targets[i + 1]));
  return result;
};

///////////////////////////////////////////////////////////////////////////////
// RegExp Extensions
ss.regexpEscape = function (s) {
  return s.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
};

///////////////////////////////////////////////////////////////////////////////
// Debug Extensions

ss.Debug = global.Debug || function () { };
ss.Debug.__typeName = 'Debug';

if (!ss.Debug.writeln) {
  ss.Debug.writeln = function (text) {
    if (global.console) {
      if (global.console.debug) {
        global.console.debug(text);
        return;
      }
      else if (global.console.log) {
        global.console.log(text);
        return;
      }
    }
    else if (global.opera &&
      global.opera.postError) {
      global.opera.postError(text);
      return;
    }
  }
};

ss.Debug._fail = function (message) {
  ss.Debug.writeln(message);
  debugger;
};

ss.Debug.assert = function (condition, message) {
  if (!condition) {
    message = 'Assert failed: ' + message;
    if (confirm(message + '\r\n\r\nBreak into debugger?')) {
      ss.Debug._fail(message);
    }
  }
};

ss.Debug.fail = function (message) {
  ss.Debug._fail(message);
};

///////////////////////////////////////////////////////////////////////////////
// Enum

var ss_Enum = ss.Enum = ss.mkType(ss, 'ss.Enum',
  {
    parse: function (enumType, s) {
      var values = enumType.prototype;
      var f;
      if (!ss.isFlags(enumType)) {
        for (f in values) {
          if (values.hasOwnProperty(f)) {
            if (f === s) {
              return values[f];
            }
          }
        }
      }
      else {
        var parts = s.split('|');
        var value = 0;
        var parsed = true;

        for (var i = parts.length - 1; i >= 0; i--) {
          var part = parts[i].trim();
          var found = false;

          for (f in values) {
            if (values.hasOwnProperty(f)) {
              if (f === part) {
                value |= values[f];
                found = true;
                break;
              }
            }
          }
          if (!found) {
            parsed = false;
            break;
          }
        }

        if (parsed) {
          return value;
        }
      }
      throw new ss_ArgumentException('Invalid Enumeration Value');
    },

    toString: function (enumType, value) {
      var values = enumType.prototype;
      var i;
      if (!ss.isFlags(enumType) || (value === 0)) {
        for (i in values) {
          if (values.hasOwnProperty(i)) {
            if (values[i] === value) {
              return i;
            }
          }
        }
        throw new ss_ArgumentException('Invalid Enumeration Value');
      }
      else {
        var parts = [];
        for (i in values) {
          if (values.hasOwnProperty(i)) {
            if (values[i] & value) {
              ss.add(parts, i);
            }
          }
        }
        if (!parts.length) {
          throw new ss_ArgumentException('Invalid Enumeration Value');
        }
        return parts.join(' | ');
      }
    },
    getValues: function (enumType) {
      var parts = [];
      var values = enumType.prototype;
      for (var i in values) {
        if (values.hasOwnProperty(i))
          parts.push(values[i]);
      }
      return parts;
    }
  }
);

ss.initClass(ss_Enum);

///////////////////////////////////////////////////////////////////////////////
// CultureInfo

var ss_CultureInfo = ss.CultureInfo = ss.mkType(ss, 'ss.CultureInfo',
  function (name, numberFormat, dateTimeFormat) {
    this.name = name;
    this.numberFormat = numberFormat;
    this.dateTimeFormat = dateTimeFormat;
  },
  {
    getFormat: function (type) {
      switch (type) {
        case ss_NumberFormatInfo: return this.numberFormat;
        case ss_DateTimeFormatInfo: return this.dateTimeFormat;
        default: return null;
      }
    }
  }
);

ss.initClass(ss_CultureInfo, null, [ss_IFormatProvider]);

ss_CultureInfo.invariantCulture = new ss_CultureInfo('en-US', ss_NumberFormatInfo.invariantInfo, ss_DateTimeFormatInfo.invariantInfo);
ss_CultureInfo.currentCulture = ss_CultureInfo.invariantCulture;

///////////////////////////////////////////////////////////////////////////////
// IEnumerator

var ss_IEnumerator = ss.IEnumerator = ss.mkType(ss, 'ss.IEnumerator');
ss.initInterface(ss_IEnumerator, { current: null, moveNext: null, reset: null }, [ss_IDisposable]);

///////////////////////////////////////////////////////////////////////////////
// IEnumerable

var ss_IEnumerable = ss.IEnumerable = ss.mkType(ss, 'ss.IEnumerable');
ss.initInterface(ss_IEnumerable, { getEnumerator: null });

ss.getEnumerator = function (obj) {
  return obj.getEnumerator ? obj.getEnumerator() : new ss_ArrayEnumerator(obj);
};

///////////////////////////////////////////////////////////////////////////////
// ICollection

var ss_ICollection = ss.ICollection = ss.mkType(ss, 'ss.ICollection');
ss.initInterface(
  ss_ICollection,
  { get_count: null, add: null, clear: null, remove: null, contains: null },
  [ss_IEnumerable]);

ss.count = function (obj) {
  return obj.get_count ? obj.get_count() : obj.length;
};

ss.add = function (obj, item) {
  if (obj.add)
    obj.add(item);
  else if (ss.isArray(obj))
    obj.push(item);
  else
    throw new ss_NotSupportedException();
};

ss.clear = function (obj) {
  if (obj.clear)
    obj.clear();
  else if (ss.isArray(obj))
    obj.length = 0;
  else
    throw new ss_NotSupportedException();
};

ss.remove = function (obj, item) {
  if (obj.remove)
    return obj.remove(item);
  else if (ss.isArray(obj)) {
    var index = ss.indexOf(obj, item);
    if (index >= 0) {
      obj.splice(index, 1);
      return true;
    }
    return false;
  }
  else
    throw new ss_NotSupportedException();
};

ss.contains = function (obj, item) {
  if (obj.contains)
    return obj.contains(item);
  else
    return ss.indexOf(obj, item) >= 0;
};

///////////////////////////////////////////////////////////////////////////////
// IReadOnlyCollection

var ss_IReadOnlyCollection = ss.IReadOnlyCollection = ss.mkType(ss, 'ss.IReadOnlyCollection');
ss.initInterface(ss_IReadOnlyCollection, { get_count: null, contains: null }, [ss_IEnumerable]);

  //#include "TimeSpan.js"

///////////////////////////////////////////////////////////////////////////////
// IEqualityComparer

var ss_IEqualityComparer = ss.IEqualityComparer = ss.mkType(ss, 'ss.IEqualityComparer');
ss.initInterface(ss_IEqualityComparer, { areEqual: null, getObjectHashCode: null });

///////////////////////////////////////////////////////////////////////////////
// IComparer

var ss_IComparer = ss.IComparer = ss.mkType(ss, 'ss.IComparer');
ss.initInterface(ss_IComparer, { compare: null });

///////////////////////////////////////////////////////////////////////////////
// Nullable

ss.unbox = function (instance) {
  if (!ss.isValue(instance))
    throw new ss_InvalidOperationException('Nullable object must have a value.');
  return instance;
};

var ss_Nullable$1 = ss.Nullable$1 = ss.mkType(ss, 'ss.Nullable$1',
  function (T) {
    var $type = ss.registerGenericClassInstance(ss_Nullable$1, [T], null, {}, {
      isInstanceOfType: function (instance) {
        return ss.isInstanceOfType(instance, T);
      }
    });
    return $type;
  },
  null,
  {
    eq: function (a, b) {
      return !ss.isValue(a) ? !ss.isValue(b) : (a === b);
    },
    ne: function (a, b) {
      return !ss.isValue(a) ? ss.isValue(b) : (a !== b);
    },
    le: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) && a <= b;
    },
    ge: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) && a >= b;
    },
    lt: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) && a < b;
    },
    gt: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) && a > b;
    },
    sub: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a - b : null;
    },
    add: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a + b : null;
    },
    mod: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a % b : null;
    },
    div: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a / b : null;
    },
    mul: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a * b : null;
    },
    band: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a & b : null;
    },
    bor: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a | b : null;
    },
    bxor: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a ^ b : null;
    },
    shl: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a << b : null;
    },
    srs: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a >> b : null;
    },
    sru: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? a >>> b : null;
    },
    and: function (a, b) {
      if (a === true && b === true)
        return true;
      else if (a === false || b === false)
        return false;
      else
        return null;
    },
    or: function (a, b) {
      if (a === true || b === true)
        return true;
      else if (a === false && b === false)
        return false;
      else
        return null;
    },
    xor: function (a, b) {
      return ss.isValue(a) && ss.isValue(b) ? !!(a ^ b) : null;
    },
    not: function (a) {
      return ss.isValue(a) ? !a : null;
    },
    neg: function (a) {
      return ss.isValue(a) ? -a : null;
    },
    pos: function (a) {
      return ss.isValue(a) ? +a : null;
    },
    cpl: function (a) {
      return ss.isValue(a) ? ~a : null;
    },
    lift1: function (f, o) {
      return ss.isValue(o) ? f(o) : null;
    },
    lift2: function (f, a, b) {
      return ss.isValue(a) && ss.isValue(b) ? f(a, b) : null;
    },
    liftcmp: function (f, a, b) {
      return ss.isValue(a) && ss.isValue(b) ? f(a, b) : false;
    },
    lifteq: function (f, a, b) {
      var va = ss.isValue(a), vb = ss.isValue(b);
      return (!va && !vb) || (va && vb && f(a, b));
    },
    liftne: function (f, a, b) {
      var va = ss.isValue(a), vb = ss.isValue(b);
      return (va !== vb) || (va && f(a, b));
    }
  }
);

ss.initGenericClass(ss_Nullable$1, 1);

///////////////////////////////////////////////////////////////////////////////
// IList

var ss_IList = ss.IList = ss.mkType(ss, 'ss.IList');
ss.initInterface(
  ss_IList,
  { get_item: null, set_item: null, indexOf: null, insert: null, removeAt: null },
  [ss_ICollection, ss_IEnumerable]);

ss.getItem = function (obj, index) {
  return obj.get_item ? obj.get_item(index) : obj[index];
};

ss.setItem = function (obj, index, value) {
  obj.set_item ? obj.set_item(index, value) : (obj[index] = value);
};

ss.indexOf = function (obj, item) {
  if ((!item || typeof (item.equals) !== 'function') && typeof (obj.indexOf) === 'function') {
    // use indexOf if item is null or if item does not implement an equals function
    return obj.indexOf(item);
  } else if (ss.isArrayOrTypedArray(obj)) {
    for (var i = 0; i < obj.length; i++) {
      if (ss.staticEquals(obj[i], item)) {
        return i;
      }
    }
    return -1;
  }
  else
    return obj.indexOf(item);
};

ss.insert = function (obj, index, item) {
  if (obj.insert)
    obj.insert(index, item);
  else if (ss.isArray(obj))
    obj.splice(index, 0, item);
  else
    throw new ss_NotSupportedException();
};

ss.removeAt = function (obj, index) {
  if (obj.removeAt)
    obj.removeAt(index);
  else if (ss.isArray(obj))
    obj.splice(index, 1);
  else
    throw new ss_NotSupportedException();
};

///////////////////////////////////////////////////////////////////////////////
// IReadOnlyList

var ss_IReadOnlyList = ss.IReadOnlyList = ss.mkType(ss, 'ss.IReadOnlyList');
ss.initInterface(ss_IReadOnlyList, { get_item: null }, [ss_IReadOnlyCollection, ss_IEnumerable]);

  // #include "IDictionary.js"

  // #include "IReadOnlyDictionary.js"

///////////////////////////////////////////////////////////////////////////////
// Int32
var intRE = /^\s*[+-]?[0-9]+\s*$/;

var defInt = function (name, min, max) {
  var type = ss[name] = ss.mkType(ss, 'ss.' + name,
    function () {
    },
    null,
    {
      isInstanceOfType: function (instance) {
        return typeof (instance) === 'number' && Math.round(instance, 0) === instance && instance >= min && instance <= max;
      },
      createInstance: function () {
        return 0;
      },
      parse: function (s) {
        var r = {};
        if (!type.tryParse(s, r))
          throw ss.isValue(s) ? intRE.test(s) ? new ss_OverflowException() : new ss_FormatException() : new ss_ArgumentNullException('s');
        return r.$;
      },
      tryParse: function (s, result) {
        result.$ = 0;
        if (!intRE.test(s))
          return false;
        var n = parseInt(s, 10);
        if (n < min || n > max)
          return false;
        result.$ = n;
        return true;
      }
    }
  );
  ss.initStruct(type, [ss_IEquatable, ss_IComparable, ss_IFormattable]);
  return type;
};

var ss_Byte = defInt('Byte', 0, 255);
var ss_SByte = defInt('SByte', -128, 127);
var ss_Int16 = defInt('Int16', -32768, 32767);
var ss_UInt16 = defInt('UInt16', 0, 65535);
var ss_Int32 = defInt('Int32', -2147483648, 2147483647);
var ss_UInt32 = defInt('UInt32', 0, 4294967295);
var ss_Int64 = defInt('Int64', -9223372036854775808, 9223372036854775807);
var ss_UInt64 = defInt('UInt64', 0, 18446744073709551615);
var ss_Char = defInt('Char', 0, 65535);

ss_Char.tryParse = function (s, result) {
  var b = s && s.length === 1;
  result.$ = b ? s.charCodeAt(0) : 0;
  return b;
};

ss_Char.parse = function (s) {
  if (!ss.isValue(s))
    throw new ss_ArgumentNullException('s');
  if (s.length !== 1)
    throw new ss_FormatException();
  return s.charCodeAt(0);
};

ss.sxb = function (x) {
  return x | (x & 0x80 ? 0xffffff00 : 0);
};

ss.sxs = function (x) {
  return x | (x & 0x8000 ? 0xffff0000 : 0);
};

ss.clip8 = function (x) {
  return ss.isValue(x) ? ss.sxb(x & 0xff) : null;
};

ss.clipu8 = function (x) {
  return ss.isValue(x) ? x & 0xff : null;
};

ss.clip16 = function (x) {
  return ss.isValue(x) ? ss.sxs(x & 0xffff) : null;
};

ss.clipu16 = function (x) {
  return ss.isValue(x) ? x & 0xffff : null;
};

ss.clip32 = function (x) {
  return ss.isValue(x) ? x | 0 : null;
};

ss.clipu32 = function (x) {
  return ss.isValue(x) ? x >>> 0 : null;
};

ss.clip64 = function (x) {
  return ss.isValue(x) ? (Math.floor(x / 0x100000000) | 0) * 0x100000000 + (x >>> 0) : null;
};

ss.clipu64 = function (x) {
  return ss.isValue(x) ? (Math.floor(x / 0x100000000) >>> 0) * 0x100000000 + (x >>> 0) : null;
};

ss.ck = function (x, tp) {
  if (ss.isValue(x) && !tp.isInstanceOfType(x))
    throw new ss_OverflowException();
  return x;
};

ss.trunc = function (n) {
  return ss.isValue(n) ? (n > 0 ? Math.floor(n) : Math.ceil(n)) : null;
};

ss.idiv = function (a, b) {
  if (!ss.isValue(a) || !ss.isValue(b)) return null;
  if (!b) throw new ss_DivideByZeroException();
  return ss.trunc(a / b);
};

ss.imod = function (a, b) {
  if (!ss.isValue(a) || !ss.isValue(b)) return null;
  if (!b) throw new ss_DivideByZeroException();
  return a % b;
};

///////////////////////////////////////////////////////////////////////////////
// MutableDateTime

var ss_JsDate = ss.JsDate = ss.mkType(ss, 'ss.JsDate',
  function () {
  },
  null,
  {
    createInstance: function () {
      return new Date();
    },
    isInstanceOfType: function (instance) {
      return instance instanceof Date;
    }
  }
);

ss.initClass(ss_JsDate, null, [ss_IEquatable, ss_IComparable]);

///////////////////////////////////////////////////////////////////////////////
// ArrayEnumerator

var ss_ArrayEnumerator = ss.ArrayEnumerator = ss.mkType(ss, 'ss.ArrayEnumerator',
  function (array) {
    this._array = array;
    this._index = -1;
  },
  {
    moveNext: function () {
      this._index++;
      return (this._index < this._array.length);
    },
    reset: function () {
      this._index = -1;
    },
    current: function () {
      if (this._index < 0 || this._index >= this._array.length)
        throw 'Invalid operation';
      return this._array[this._index];
    },
    dispose: function () {
    }
  }
);

ss.initClass(ss_ArrayEnumerator, null, [ss_IEnumerator, ss_IDisposable]);

///////////////////////////////////////////////////////////////////////////////
// ObjectEnumerator

var ss_ObjectEnumerator = ss.ObjectEnumerator = ss.mkType(ss, 'ss.ObjectEnumerator',
  function (o) {
    this._keys = Object.keys(o);
    this._index = -1;
    this._object = o;
  },
  {
    moveNext: function () {
      this._index++;
      return (this._index < this._keys.length);
    },
    reset: function () {
      this._index = -1;
    },
    current: function () {
      if (this._index < 0 || this._index >= this._keys.length)
        throw new ss_InvalidOperationException('Invalid operation');
      var k = this._keys[this._index];
      return { key: k, value: this._object[k] };
    },
    dispose: function () {
    }
  }
);

ss.initClass(ss_ObjectEnumerator, null, [ss_IEnumerator, ss_IDisposable]);

///////////////////////////////////////////////////////////////////////////////
// EqualityComparer

var ss_EqualityComparer = ss.EqualityComparer = ss.mkType(ss, 'ss.EqualityComparer',
  function () {
  },
  {
    areEqual: function (x, y) {
      return ss.staticEquals(x, y);
    },
    getObjectHashCode: function (obj) {
      return ss.isValue(obj) ? ss.getHashCode(obj) : 0;
    }
  }
);

ss.initClass(ss_EqualityComparer, null, [ss_IEqualityComparer]);
ss_EqualityComparer.def = new ss_EqualityComparer();

///////////////////////////////////////////////////////////////////////////////
// Comparer

var ss_Comparer = ss.Comparer = ss.mkType(ss, 'ss.Comparer',
  function (f) {
    this.f = f;
  },
  {
    compare: function (x, y) {
      return this.f(x, y);
    }
  }
);

ss.initClass(ss_Comparer, null, [ss_IComparer]);

ss_Comparer.def = new ss_Comparer(function (a, b) {
  if (!ss.isValue(a))
    return !ss.isValue(b) ? 0 : -1;
  else if (!ss.isValue(b))
    return 1;
  else
    return ss.compare(a, b);
});

///////////////////////////////////////////////////////////////////////////////
// KeyValuePair

var ss_KeyValuePair = ss.KeyValuePair = ss.mkType(ss, 'ss.KeyValuePair',
  function () {
  },
  null,
  {
    createInstance: function () {
      return { key: null, value: null };
    },
    isInstanceOfType: function (o) {
      return typeof o === 'object' && 'key' in o && 'value' in o;
    }
  }
);

ss.initStruct(ss_KeyValuePair);

  //#include "Dictionary.js"

///////////////////////////////////////////////////////////////////////////////
// IDisposable

var ss_IDisposable = ss.IDisposable = ss.mkType(ss, 'ss.IDisposable');
ss.initInterface(ss_IDisposable, { dispose: null });

///////////////////////////////////////////////////////////////////////////////
// StringBuilder

var ss_StringBuilder = ss.StringBuilder = ss.mkType(ss, 'ss.StringBuilder',
  function (s) {
    this._parts = (ss.isValue(s) && s !== '') ? [s] : [];
    this.length = ss.isValue(s) ? s.length : 0;
  },
  {
    append: function (o) {
      if (ss.isValue(o)) {
        var s = o.toString();
        ss.add(this._parts, s);
        this.length += s.length;
      }
      return this;
    },

    appendChar: function (c) {
      return this.append(String.fromCharCode(c));
    },

    appendLine: function (s) {
      this.append(s);
      this.append('\r\n');
      return this;
    },

    appendLineChar: function (c) {
      return this.appendLine(String.fromCharCode(c));
    },

    clear: function () {
      this._parts = [];
      this.length = 0;
    },

    toString: function () {
      return this._parts.join('');
    }
  }
);

ss.initClass(ss_StringBuilder);

///////////////////////////////////////////////////////////////////////////////
// Random

var ss_Random = ss.Random = ss.mkType(ss, 'ss.Random',
  function (seed) {
    var _seed = (seed === undefined) ? parseInt(Date.now() % 2147483648) : parseInt(Math.abs(seed));
    this.inext = 0;
    this.inextp = 21;
    this.seedArray = new Array(56);
    var i;
    for (i = 0; i < 56; i++)
      this.seedArray[i] = 0;

    _seed = 161803398 - _seed;
    if (_seed < 0)
      _seed += 2147483648;
    this.seedArray[55] = _seed;
    var mk = 1;
    for (i = 1; i < 55; i++) {
      var ii = (21 * i) % 55;
      this.seedArray[ii] = mk;
      mk = _seed - mk;
      if (mk < 0)
        mk += 2147483648;

      _seed = this.seedArray[ii];
    }
    for (var j = 1; j < 5; j++) {
      for (var k = 1; k < 56; k++) {
        this.seedArray[k] -= this.seedArray[1 + (k + 30) % 55];
        if (this.seedArray[k] < 0)
          this.seedArray[k] += 2147483648;
      }
    }
  },
  {
    next: function () {
      return this.sample() * 2147483648 | 0;
    },
    nextMax: function (max) {
      return this.sample() * max | 0;
    },
    nextMinMax: function (min, max) {
      return (this.sample() * (max - min) + min) | 0;
    },
    nextBytes: function (bytes) {
      for (var i = 0; i < bytes.length; i++)
        bytes[i] = (this.sample() * 256) | 0;
    },
    nextDouble: function () {
      return this.sample();
    },
    sample: function () {
      if (++this.inext >= 56)
        this.inext = 1;
      if (++this.inextp >= 56)
        this.inextp = 1;

      var retVal = this.seedArray[this.inext] - this.seedArray[this.inextp];

      if (retVal < 0)
        retVal += 2147483648;

      this.seedArray[this.inext] = retVal;

      return retVal * (1.0 / 2147483648);
    }
  }
);

ss.initClass(ss_Random);

///////////////////////////////////////////////////////////////////////////////
// EventArgs

var ss_EventArgs = ss.EventArgs = ss.mkType(ss, 'ss.EventArgs',
  function () {
  }
);

ss.initClass(ss_EventArgs);
ss_EventArgs.Empty = new ss_EventArgs();

///////////////////////////////////////////////////////////////////////////////
// Exception

var ss_Exception = ss.Exception = ss.mkType(ss, 'ss.Exception',
  function (message, innerException) {
    this._message = message || 'An error occurred.';
    this._innerException = innerException || null;
    this._error = new Error();
  },
  {
    get_message: function () {
      return this._message;
    },
    get_innerException: function () {
      return this._innerException;
    },
    get_stack: function () {
      return this._error.stack;
    },
    toString: function () {
      var message = this._message;
      var exception = this;
      if (ss.isNullOrEmptyString(message)) {
        if (ss.isValue(ss.getInstanceType(exception)) && ss.isValue(ss.getTypeFullName(ss.getInstanceType(exception)))) {
          message = ss.getTypeFullName(ss.getInstanceType(exception));
        }
        else {
          message = '[object Exception]';
        }
      }
      return message;
    }
  },
  {
    wrap: function (o) {
      if (ss.isInstanceOfType(o, ss_Exception)) {
        return o;
      }
      else if (o instanceof TypeError) {
        // TypeError can either be 'cannot read property blah of null/undefined' (proper NullReferenceException), or it can be eg. accessing a non-existent method of an object.
        // As long as all code is compiled, they should with a very high probability indicate the use of a null reference.
        return new ss_NullReferenceException(o.message, new ss_JsErrorException(o));
      }
      else if (o instanceof RangeError) {
        return new ss_ArgumentOutOfRangeException(null, o.message, new ss_JsErrorException(o));
      }
      else if (o instanceof Error) {
        return new ss_JsErrorException(o);
      }
      else {
        return new ss_Exception(o.toString());
      }
    }
  }
);

ss.initClass(ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// NotImplementedException

var ss_NotImplementedException = ss.NotImplementedException = ss.mkType(ss, 'ss.NotImplementedException',
  function (message, innerException) {
    ss_Exception.call(this, message || 'The method or operation is not implemented.', innerException);
  }
);

ss.initClass(ss_NotImplementedException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// NotSupportedException

var ss_NotSupportedException = ss.NotSupportedException = ss.mkType(ss, 'ss.NotSupportedException',
  function (message, innerException) {
    ss_Exception.call(this, message || 'Specified method is not supported.', innerException);
  }
);

ss.initClass(ss_NotSupportedException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// AggregateException

var ss_AggregateException = ss.AggregateException = ss.mkType(ss, 'ss.AggregateException',
  function (message, innerExceptions) {
    this.innerExceptions = ss.isValue(innerExceptions) ? ss.arrayFromEnumerable(innerExceptions) : [];
    ss_Exception.call(this, message || 'One or more errors occurred.', this.innerExceptions.length ? this.innerExceptions[0] : null);
  },
  {
    flatten: function () {
      var inner = [];
      for (var i = 0; i < this.innerExceptions.length; i++) {
        var e = this.innerExceptions[i];
        if (ss.isInstanceOfType(e, ss_AggregateException)) {
          inner.push.apply(inner, e.flatten().innerExceptions);
        }
        else {
          inner.push(e);
        }
      }
      return new ss_AggregateException(this._message, inner);
    }
  }
);

ss.initClass(ss_AggregateException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// PromiseException

var ss_PromiseException = ss.PromiseException = ss.mkType(ss, 'ss.PromiseException',
  function (args, message, innerException) {
    ss_Exception.call(this, message || (args.length && args[0] ? args[0].toString() : 'An error occurred'), innerException);
    this.arguments = ss.arrayClone(args);
  },
  {
    get_arguments: function () {
      return this._arguments;
    }
  }
);

ss_PromiseException.__typeName = 'ss.PromiseException';
ss.PromiseException = ss_PromiseException;
ss.initClass(ss_PromiseException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// JsErrorException

var ss_JsErrorException = ss.JsErrorException = ss.mkType(ss, 'ss.JsErrorException', function (error, message, innerException) {
  ss_Exception.call(this, message || error.message, innerException);
  this.error = error;
},
  {
    get_stack: function () {
      return this.error.stack;
    }
  }
);

ss.initClass(ss_JsErrorException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// ArgumentException

var ss_ArgumentException = ss.ArgumentException = ss.mkType(ss, 'ss.ArgumentException',
  function (message, paramName, innerException) {
    ss_Exception.call(this, message || 'Value does not fall within the expected range.', innerException);
    this.paramName = paramName || null;
  }
);

ss.initClass(ss_ArgumentException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// ArgumentNullException

var ss_ArgumentNullException = ss.ArgumentNullException = ss.mkType(ss, 'ss.ArgumentNullException',
  function (paramName, message, innerException) {
    if (!message) {
      message = 'Value cannot be null.';
      if (paramName)
        message += '\nParameter name: ' + paramName;
    }

    ss_ArgumentException.call(this, message, paramName, innerException);
  }
);

ss.initClass(ss_ArgumentNullException, ss_ArgumentException);

////////////////////////////////////////////////////////////////////////////////
// ArgumentOutOfRangeException

var ss_ArgumentOutOfRangeException = ss.ArgumentOutOfRangeException = ss.mkType(ss, 'ss.ArgumentOutOfRangeException',
  function (paramName, message, innerException, actualValue) {
    if (!message) {
      message = 'Value is out of range.';
      if (paramName)
        message += '\nParameter name: ' + paramName;
    }

    ss_ArgumentException.call(this, message, paramName, innerException);
    this.actualValue = actualValue || null;
  }
);

ss.initClass(ss_ArgumentOutOfRangeException, ss_ArgumentException);

////////////////////////////////////////////////////////////////////////////////
// FormatException

var ss_FormatException = ss.FormatException = ss.mkType(ss, 'ss.FormatException',
  function (message, innerException) {
    ss_Exception.call(this, message || 'Invalid format.', innerException);
  }
);

ss.initClass(ss_FormatException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// ArithmeticException

var ss_ArithmeticException = ss.ArithmeticException = ss.mkType(ss, 'ss.ArithmeticException',
  function (message, innerException) {
    ss_Exception.call(this, message || 'Overflow or underflow in the arithmetic operation.', innerException);
  }
);

ss.initClass(ss_ArithmeticException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// OverflowException

var ss_OverflowException = ss.OverflowException = ss.mkType(ss, 'ss.OverflowException',
  function (message, innerException) {
    ss_ArithmeticException.call(this, message || 'Arithmetic operation resulted in an overflow.', innerException);
  }
);

ss.initClass(ss_OverflowException, ss_ArithmeticException);

////////////////////////////////////////////////////////////////////////////////
// DivideByZeroException

var ss_DivideByZeroException = ss.DivideByZeroException = ss.mkType(ss, 'ss.DivideByZeroException',
  function (message, innerException) {
    ss_ArithmeticException.call(this, message || 'Division by 0.', innerException);
  }
);

ss.initClass(ss_DivideByZeroException, ss_ArithmeticException);

////////////////////////////////////////////////////////////////////////////////
// InvalidCastException

var ss_InvalidCastException = ss.InvalidCastException = ss.mkType(ss, 'ss.InvalidCastException',
  function (message, innerException) {
    ss_Exception.call(this, message || 'The cast is not valid.', innerException);
  }
);

ss.initClass(ss_InvalidCastException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// InvalidOperationException

var ss_InvalidOperationException = ss.InvalidOperationException = ss.mkType(ss, 'ss.InvalidOperationException',
  function (message, innerException) {
    ss_Exception.call(this, message || 'Operation is not valid due to the current state of the object.', innerException);
  }
);
ss.initClass(ss_InvalidOperationException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// NullReferenceException

var ss_NullReferenceException = ss.NullReferenceException = ss.mkType(ss, 'ss.NullReferenceException',
  function (message, innerException) {
    ss_Exception.call(this, message || 'Object is null.', innerException);
  }
);

ss.initClass(ss_NullReferenceException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// KeyNotFoundException

var ss_KeyNotFoundException = ss.KeyNotFoundException = ss.mkType(ss, 'ss.KeyNotFoundException',
  function (message, innerException) {
    ss_Exception.call(this, message || 'Key not found.', innerException);
  }
);
ss.initClass(ss_KeyNotFoundException, ss_Exception);

////////////////////////////////////////////////////////////////////////////////
// InvalidOperationException

var ss_AmbiguousMatchException = ss.AmbiguousMatchException = ss.mkType(ss, 'ss.AmbiguousMatchException',
  function (message, innerException) {
    ss_Exception.call(this, message || 'Ambiguous match.', innerException);
  }
);

ss.initClass(ss_AmbiguousMatchException, ss_Exception);

///////////////////////////////////////////////////////////////////////////////
// IteratorBlockEnumerable

var ss_IteratorBlockEnumerable = ss.IteratorBlockEnumerable = ss.mkType(ss, 'ss.IteratorBlockEnumerable',
  function (getEnumerator, $this) {
    this._getEnumerator = getEnumerator;
    this._this = $this;
  },
  {
    getEnumerator: function () {
      return this._getEnumerator.call(this._this);
    }
  }
);

ss.initClass(ss_IteratorBlockEnumerable, null, [ss_IEnumerable]);

///////////////////////////////////////////////////////////////////////////////
// IteratorBlockEnumerator

var ss_IteratorBlockEnumerator = ss.IteratorBlockEnumerator = ss.mkType(ss, 'ss.IteratorBlockEnumerator',
  function (moveNext, getCurrent, dispose, $this) {
    this._moveNext = moveNext;
    this._getCurrent = getCurrent;
    this._dispose = dispose;
    this._this = $this;
  },
  {
    moveNext: function () {
      try {
        return this._moveNext.call(this._this);
      }
      catch (ex) {
        if (this._dispose)
          this._dispose.call(this._this);
        throw ex;
      }
    },
    current: function () {
      return this._getCurrent.call(this._this);
    },
    reset: function () {
      throw new ss_NotSupportedException('Reset is not supported.');
    },
    dispose: function () {
      if (this._dispose)
        this._dispose.call(this._this);
    }
  }
);

ss.initClass(ss_IteratorBlockEnumerator, null, [ss_IEnumerator, ss_IDisposable]);

///////////////////////////////////////////////////////////////////////////////
// Lazy

var ss_Lazy = ss.Lazy = ss.mkType(ss, 'ss.Lazy',
  function (valueFactory) {
    this._valueFactory = valueFactory;
    this.isValueCreated = false;
  },
  {
    value: function () {
      if (!this.isValueCreated) {
        this._value = this._valueFactory();
        delete this._valueFactory;
        this.isValueCreated = true;
      }
      return this._value;
    }
  }
);

ss.initClass(ss_Lazy);

////////////////////////////////////////////////////////////////////////////////
// OperationCanceledException

var ss_OperationCanceledException = ss.OperationCanceledException = ss.mkType(ss, 'ss.OperationCanceledException',
  function (message, token, innerException) {
    ss_Exception.call(this, message || 'Operation was canceled.', innerException);
    this.cancellationToken = token || ss_CancellationToken.none;
  }
);

ss.initClass(ss_OperationCanceledException, ss_Exception);

///////////////////////////////////////////////////////////////////////////////
// CancellationTokenRegistration

var ss_CancellationTokenRegistration = ss.CancellationTokenRegistration = ss.mkType(ss, 'ss.CancellationTokenRegistration',
  function (cts, o) {
    this._cts = cts;
    this._o = o;
  },
  {
    dispose: function () {
      if (this._cts) {
        this._cts._deregister(this._o);
        this._cts = this._o = null;
      }
    },
    equalsT: function (o) {
      return this === o;
    }
  }
);

ss.initStruct(ss_CancellationTokenRegistration, [ss_IDisposable, ss_IEquatable]);

///////////////////////////////////////////////////////////////////////////////
// CancellationTokenSource

var ss_CancellationTokenSource = ss.CancellationTokenSource = ss.mkType(ss, 'ss.CancellationTokenSource',
  function (delay) {
    this._timeout = typeof delay === 'number' && delay >= 0 ? setTimeout(ss.mkdel(this, 'cancel'), delay, -1) : null;
    this.isCancellationRequested = false;
    this.token = new ss_CancellationToken(this);
    this._handlers = [];
  },
  {
    cancel: function (throwFirst) {
      if (this.isCancellationRequested)
        return;

      this.isCancellationRequested = true;
      var x = [];
      var h = this._handlers;

      this._clean();

      for (var i = 0; i < h.length; i++) {
        try {
          h[i].f(h[i].s);
        }
        catch (ex) {
          if (throwFirst && throwFirst !== -1)
            throw ex;
          x.push(ex);
        }
      }
      if (x.length > 0 && throwFirst !== -1)
        throw new ss_AggregateException(null, x);
    },
    cancelAfter: function (delay) {
      if (this.isCancellationRequested)
        return;
      if (this._timeout)
        clearTimeout(this._timeout);
      this._timeout = setTimeout(ss.mkdel(this, 'cancel'), delay, -1);
    },
    _register: function (f, s) {
      if (this.isCancellationRequested) {
        f(s);
        return new ss_CancellationTokenRegistration();
      }
      else {
        var o = { f: f, s: s };
        this._handlers.push(o);
        return new ss_CancellationTokenRegistration(this, o);
      }
    },
    _deregister: function (o) {
      var ix = this._handlers.indexOf(o);
      if (ix >= 0)
        this._handlers.splice(ix, 1);
    },
    dispose: function () {
      this._clean();
    },
    _clean: function () {
      if (this._timeout)
        clearTimeout(this._timeout);
      this._timeout = null;
      this._handlers = [];
      if (this._links) {
        for (var i = 0; i < this._links.length; i++)
          this._links[i].dispose();
        this._links = null;
      }
    }
  },
  {
    createLinked: function () {
      var cts = new ss_CancellationTokenSource();
      cts._links = [];
      var d = ss.mkdel(cts, 'cancel');
      for (var i = 0; i < arguments.length; i++) {
        cts._links.push(arguments[i].register(d));
      }
      return cts;
    }
  }
);

ss.initClass(ss_CancellationTokenSource, null, [ss_IDisposable]);

///////////////////////////////////////////////////////////////////////////////
// CancellationToken

var ss_CancellationToken = ss.CancellationToken = ss.mkType(ss, 'ss.CancellationToken',
  function (source) {
    if (!(source instanceof ss_CancellationTokenSource))
      source = source ? ss_CancellationToken._sourceTrue : ss_CancellationToken._sourceFalse;
    this._source = source;
  },
  {
    get_canBeCanceled: function () {
      return !this._source._uncancellable;
    },
    get_isCancellationRequested: function () {
      return this._source.isCancellationRequested;
    },
    throwIfCancellationRequested: function () {
      if (this._source.isCancellationRequested)
        throw new ss_OperationCanceledException(this);
    },
    register: function (cb, s) {
      return this._source._register(cb, s);
    }
  },
  {
    _sourceTrue: { isCancellationRequested: true, _register: function (f, s) { f(s); return new ss_CancellationTokenRegistration(); } },
    _sourceFalse: { _uncancellable: true, isCancellationRequested: false, _register: function () { return new ss_CancellationTokenRegistration(); } }
  }
);
ss_CancellationToken.none = new ss_CancellationToken();

ss.initStruct(ss_CancellationToken);

////////////////////////////////////////////////////////////////////////////////
// TaskCanceledException

var ss_TaskCanceledException = ss.TaskCanceledException = ss.mkType(ss, 'ss.TaskCanceledException',
  function (message, task, innerException) {
    ss_OperationCanceledException.call(this, message || 'A task was canceled.', null, innerException);
    this.task = task || null;
  }
);

ss.initClass(ss_TaskCanceledException, ss_OperationCanceledException);

///////////////////////////////////////////////////////////////////////////////
// Task

var ss_Task = ss.Task = ss.mkType(ss, 'ss.Task',
  function (action, state) {
    this._action = action;
    this._state = state;
    this.exception = null;
    this.status = 0;
    this._thens = [];
    this._result = null;
  },
  {
    continueWith: function (continuation) {
      var tcs = new ss_TaskCompletionSource();
      var _this = this;
      var fn = function () {
        try {
          tcs.setResult(continuation(_this));
        }
        catch (e) {
          tcs.setException(ss_Exception.wrap(e));
        }
      };
      if (this.isCompleted()) {
        setTimeout(fn, 0);
      }
      else {
        this._thens.push(fn);
      }
      return tcs.task;
    },
    start: function () {
      if (this.status !== 0)
        throw new ss_InvalidOperationException('Task was already started.');
      var _this = this;
      this.status = 3;
      setTimeout(function () {
        try {
          var result = _this._action(_this._state);
          delete _this._action;
          delete _this._state;
          _this._complete(result);
        }
        catch (e) {
          _this._fail(new ss_AggregateException(null, [ss_Exception.wrap(e)]));
        }
      }, 0);
    },
    _runCallbacks: function () {
      for (var i = 0; i < this._thens.length; i++)
        this._thens[i](this);
      delete this._thens;
    },
    _complete: function (result) {
      if (this.isCompleted())
        return false;
      this._result = result;
      this.status = 5;
      this._runCallbacks();
      return true;
    },
    _fail: function (exception) {
      if (this.isCompleted())
        return false;
      this.exception = exception;
      this.status = 7;
      this._runCallbacks();
      return true;
    },
    _cancel: function () {
      if (this.isCompleted())
        return false;
      this.status = 6;
      this._runCallbacks();
      return true;
    },
    isCanceled: function () {
      return this.status === 6;
    },
    isCompleted: function () {
      return this.status >= 5;
    },
    isFaulted: function () {
      return this.status === 7;
    },
    _getResult: function (shouldAwait) {
      switch (this.status) {
        case 5:
          return this._result;
        case 6:
          var ex = new ss_TaskCanceledException(null, this);
          throw shouldAwait ? ex : new ss_AggregateException(null, [ex]);
        case 7:
          throw shouldAwait ? this.exception.innerExceptions[0] : this.exception;
        default:
          throw new ss_InvalidOperationException('Task is not yet completed.');
      }
    },
    getResult: function () {
      return this._getResult(false);
    },
    getAwaitedResult: function () {
      return this._getResult(true);
    },
    dispose: function () {
    }
  },
  {
    delay: function (delay) {
      var tcs = new ss_TaskCompletionSource();
      setTimeout(function () {
        tcs.setResult(0);
      }, delay);
      return tcs.task;
    },
    fromResult: function (result) {
      var t = new ss_Task();
      t.status = 5;
      t._result = result;
      return t;
    },
    run: function (f) {
      var tcs = new ss_TaskCompletionSource();
      setTimeout(function () {
        try {
          tcs.setResult(f());
        }
        catch (e) {
          tcs.setException(ss_Exception.wrap(e));
        }
      }, 0);
      return tcs.task;
    },
    whenAll: function (tasks) {
      var tcs = new ss_TaskCompletionSource();
      if (tasks.length === 0) {
        tcs.setResult([]);
      }
      else {
        var result = new Array(tasks.length), remaining = tasks.length, cancelled = false, exceptions = [];
        for (var i = 0; i < tasks.length; i++) {
          (function (i) {
            tasks[i].continueWith(function (t) {
              switch (t.status) {
                case 5:
                  result[i] = t.getResult();
                  break;
                case 6:
                  cancelled = true;
                  break;
                case 7:
                  ss.arrayAddRange(exceptions, t.exception.innerExceptions);
                  break;
                default:
                  throw new ss_InvalidOperationException('Invalid task status ' + t.status);
              }
              if (--remaining === 0) {
                if (exceptions.length > 0)
                  tcs.setException(exceptions);
                else if (cancelled)
                  tcs.setCanceled();
                else
                  tcs.setResult(result);
              }
            });
          })(i);
        }
      }
      return tcs.task;
    },
    whenAny: function (tasks) {
      if (!tasks.length)
        throw new ss_ArgumentException('Must wait for at least one task', 'tasks');

      var tcs = new ss_TaskCompletionSource();
      for (var i = 0; i < tasks.length; i++) {
        tasks[i].continueWith(function (t) {
          switch (t.status) {
            case 5:
              tcs.trySetResult(t);
              break;
            case 6:
              tcs.trySetCanceled();
              break;
            case 7:
              tcs.trySetException(t.exception.innerExceptions);
              break;
            default:
              throw new ss_InvalidOperationException('Invalid task status ' + t.status);
          }
        });
      }
      return tcs.task;
    },
    fromDoneCallback: function (t, i, m) {
      var tcs = new ss_TaskCompletionSource(), args;
      if (typeof (i) === 'number') {
        args = Array.prototype.slice.call(arguments, 3);
        if (i < 0)
          i += args.length + 1;
      }
      else {
        args = Array.prototype.slice.call(arguments, 2);
        m = i;
        i = args.length;
      }

      var cb = function (v) {
        tcs.setResult(v);
      };

      args = args.slice(0, i).concat(cb, args.slice(i));

      t[m].apply(t, args);
      return tcs.task;
    },
    fromPromise: function (p, f) {
      var tcs = new ss_TaskCompletionSource();
      if (typeof (f) === 'number')
        f = (function (i) { return function () { return arguments[i >= 0 ? i : (arguments.length + i)]; }; })(f);
      else if (typeof (f) !== 'function')
        f = function () { return Array.prototype.slice.call(arguments, 0); };

      p.then(function () {
        tcs.setResult(typeof (f) === 'function' ? f.apply(null, arguments) : null);
      }, function () {
        tcs.setException(new ss_PromiseException(Array.prototype.slice.call(arguments, 0)));
      });
      return tcs.task;
    },
    fromNode: function (t, f, m) {
      var tcs = new ss_TaskCompletionSource(), args;
      if (typeof (f) === 'function') {
        args = Array.prototype.slice.call(arguments, 3);
      }
      else {
        args = Array.prototype.slice.call(arguments, 2);
        m = f;
        f = function () { return arguments[0]; };
      }

      var cb = function (e) {
        if (e)
          tcs.setException(ss_Exception.wrap(e));
        else
          tcs.setResult(f.apply(null, Array.prototype.slice.call(arguments, 1)));
      };

      args.push(cb);

      t[m].apply(t, args);
      return tcs.task;
    }
  }
);

ss.initClass(ss_Task, null, [ss_IDisposable]);

////////////////////////////////////////////////////////////////////////////////
// TaskStatus

var ss_TaskStatus = ss.TaskStatus = ss.mkEnum(ss, 'ss.TaskStatus', { created: 0, running: 3, ranToCompletion: 5, canceled: 6, faulted: 7 });

///////////////////////////////////////////////////////////////////////////////
// TaskCompletionSource

var ss_TaskCompletionSource = ss.TaskCompletionSource = ss.mkType(ss, 'ss.TaskCompletionSource',
  function () {
    this.task = new ss_Task();
    this.task.status = 3;
  },
  {
    setCanceled: function () {
      if (!this.task._cancel())
        throw new ss_InvalidOperationException('Task was already completed.');
    },
    setResult: function (result) {
      if (!this.task._complete(result))
        throw new ss_InvalidOperationException('Task was already completed.');
    },
    setException: function (exception) {
      if (!this.trySetException(exception))
        throw new ss_InvalidOperationException('Task was already completed.');
    },
    trySetCanceled: function () {
      return this.task._cancel();
    },
    trySetResult: function (result) {
      return this.task._complete(result);
    },
    trySetException: function (exception) {
      if (ss.isInstanceOfType(exception, ss_Exception))
        exception = [exception];
      return this.task._fail(new ss_AggregateException(null, exception));
    }
  }
);

ss.initClass(ss_TaskCompletionSource);

///////////////////////////////////////////////////////////////////////////////
// CancelEventArgs

var ss_CancelEventArgs = ss.CancelEventArgs = ss.mkType(ss, 'ss.CancelEventArgs',
  function () {
    ss_EventArgs.call(this);
    this.cancel = false;
  }
);

ss.initClass(ss_CancelEventArgs, ss_EventArgs);

  //#include "Guid.js"

  global.ss = ss;
})(global);
