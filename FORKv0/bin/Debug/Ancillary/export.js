/*
Plugin Name: amCharts Export
Description: Adds export capabilities to amCharts products
Author: Benjamin Maertz, amCharts
Version: 1.1.8
Author URI: http://www.amcharts.com/

Copyright 2015 amCharts

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

Please note that the above license covers only this plugin. It by all means does
not apply to any other amCharts products that are covered by different licenses.
*/

/*
 ** Polyfill translation
 */
if ( !AmCharts.translations[ "export" ] ) {
	AmCharts.translations[ "export" ] = {}
}
if ( !AmCharts.translations[ "export" ][ "en" ] ) {
	AmCharts.translations[ "export" ][ "en" ] = {
		"fallback.save.text": "CTRL + C to copy the data into the clipboard.",
		"fallback.save.image": "Rightclick -> Save picture as... to save the image.",
		"capturing.delayed.menu.label": "{{duration}}",
		"capturing.delayed.menu.title": "Click to cancel"
	}
}

/**
 * Set init handler
 */
AmCharts.addInitHandler( function( chart ) {
	var _this = {
		name: "export",
		version: "1.1.8",
		libs: {
			async: true,
			autoLoad: true,
			reload: false,
			path: ( ( chart.path || "" ) + "" ),
			resources: [ {
				"pdfmake.js": [ "vfs_fonts.js" ],
				"jszip.js": [ "xlsx.js" ]
			}, "fabric.js/fabric.js", "FileSaver.js/FileSaver.js" ]
		},
		config: {},
		setup: {
			hasBlob: false
		},
		drawing: {
			enabled: false,
			actions: [ "undo", "redo", "done", "cancel" ],
			undos: [],
			undo: function() {
				var item = _this.drawing.undos.pop();
				if ( item ) {
					_this.drawing.redos.push( item );
					if ( item.action == "added" ) {
						item.target.known = true;
						_this.setup.fabric.remove( item.target );
					}
					item.target.setOptions( JSON.parse( item.options ) );
					_this.setup.fabric.renderAll();
				}
			},
			redos: [],
			redo: function() {
				var item = _this.drawing.redos.pop();
				if ( item ) {
					_this.drawing.undos.push( item );
					if ( item.action == "added" ) {
						item.target.known = true;
						_this.setup.fabric.add( item.target );
					}
					item.target.setOptions( JSON.parse( item.options ) );
					_this.setup.fabric.renderAll();
				}
			},
			done: function() {
				_this.drawing.enabled = false;
				_this.drawing.undos = [];
				_this.drawing.redos = [];
				_this.createMenu( _this.config.menu );
				_this.setup.wrapper.setAttribute( "class", _this.setup.chart.classNamePrefix + "-export-canvas" );
			}
		},
		defaults: {
			position: "top-right",
			fileName: "amCharts",
			action: "download",
			formats: {
				JPG: {
					mimeType: "image/jpg",
					extension: "jpg",
					capture: true
				},
				PNG: {
					mimeType: "image/png",
					extension: "png",
					capture: true
				},
				SVG: {
					mimeType: "text/xml",
					extension: "svg",
					capture: true
				},
				PDF: {
					mimeType: "application/pdf",
					extension: "pdf",
					capture: true
				},
				CSV: {
					mimeType: "text/plain",
					extension: "csv"
				},
				JSON: {
					mimeType: "text/plain",
					extension: "json"
				},
				XLSX: {
					mimeType: "application/octet-stream",
					extension: "xlsx"
				}
			},
			fabric: {
				backgroundColor: "#FFFFFF",
				isDrawingMode: false,
				selection: false,
				removeImages: true
			},
			pdfMake: {
				pageSize: "A4",
				pageOrientation: "portrait",
				images: {},
				content: [ {
					image: "reference",
					fit: [ 523.28, 769.89 ]
				} ]
			},
			divId: null,
			menuReviver: null,
			menuWalker: null,
			menu: [ {
				"class": "export-main",
				label: "Export",
				menu: [ {
					label: "Download as ...",
					menu: [ "PNG", "JPG", "SVG", {
						format: "PDF",
						content: [ "Saved from:", window.location.href, {
							image: "reference",
							fit: [ 523.28, 769.89 ] // FIT IMAGE TO A4
						} ]
					} ]
				}, {
					label: "Save data ...",
					menu: [ "CSV", "XLSX", "JSON" ]
				}, {
					label: "Annotate",
					action: "draw",
					menu: [ {
						"class": "export-drawing",
						menu: [ {
							label: "Color ...",
							menu: [ {
								"class": "export-drawing-color export-drawing-color-black",
								label: "Black",
								click: function() {
									this.setup.fabric.freeDrawingBrush.color = "#000";
								}
							}, {
								"class": "export-drawing-color export-drawing-color-white",
								label: "White",
								click: function() {
									this.setup.fabric.freeDrawingBrush.color = "#fff";
								}
							}, {
								"class": "export-drawing-color export-drawing-color-red",
								label: "Red",
								click: function() {
									this.setup.fabric.freeDrawingBrush.color = "#f00";
								}
							}, {
								"class": "export-drawing-color export-drawing-color-green",
								label: "Green",
								click: function() {
									this.setup.fabric.freeDrawingBrush.color = "#0f0";
								}
							}, {
								"class": "export-drawing-color export-drawing-color-blue",
								label: "Blue",
								click: function() {
									this.setup.fabric.freeDrawingBrush.color = "#00f";
								}
							} ]
						}, "UNDO", "REDO", {
							label: "Save as ...",
							menu: [ "PNG", "JPG", "SVG", {
								format: "PDF",
								content: [ "Saved from:", window.location.href, {
									image: "reference",
									fit: [ 523.28, 769.89 ] // FIT IMAGE TO A4
								} ]
							} ]
						}, {
							format: "PRINT",
							label: "Print"
						}, "CANCEL" ]
					} ]
				}, {
					format: "PRINT",
					label: "Print"
				} ]
			} ],
			fallback: true
		},

		/**
		 * Returns translated message, takes english as default
		 */
		i18l: function( key, language ) {
			var catalog = AmCharts.translations[ "export" ][ language ] || AmCharts.translations[ "export" ][ "en" ];
			return catalog[ key ] || key;
		},

		/**
		 * Generates download file; if unsupported offers fallback to save manually
		 */
		download: function( data, type, filename ) {
			// SAVE
			if ( window.saveAs && _this.setup.hasBlob ) {
				var blob = _this.toBlob( {
					data: data,
					type: type
				}, function( data ) {
					saveAs( data, filename );
				} );

				// FALLBACK TEXTAREA
			} else if ( _this.config.fallback && type == "text/plain" ) {
				var div = document.createElement( "div" );
				var msg = document.createElement( "div" );
				var textarea = document.createElement( "textarea" );

				msg.innerHTML = _this.i18l( "fallback.save.text", _this.setup.chart.language );

				div.appendChild( msg );
				div.appendChild( textarea );
				msg.setAttribute( "class", "amcharts-export-fallback-message" );
				div.setAttribute( "class", "amcharts-export-fallback" );
				_this.setup.chart.containerDiv.appendChild( div );

				// FULFILL TEXTAREA AND PRESELECT
				textarea.setAttribute( "readonly", "" );
				textarea.value = data;
				textarea.focus();
				textarea.select();

				// UPDATE MENU
				_this.createMenu( [ {
					"class": "export-main export-close",
					label: "Done",
					click: function() {
						_this.createMenu( _this.config.menu );
						_this.setup.chart.containerDiv.removeChild( div );
					}
				} ] );

				// FALLBACK IMAGE
			} else if ( _this.config.fallback && type.split( "/" )[ 0 ] == "image" ) {
				var div = document.createElement( "div" );
				var msg = document.createElement( "div" );
				var img = _this.toImage( {
					data: data
				} );

				msg.innerHTML = _this.i18l( "fallback.save.image", _this.setup.chart.language );

				// FULFILL TEXTAREA AND PRESELECT
				div.appendChild( msg );
				div.appendChild( img );
				msg.setAttribute( "class", "amcharts-export-fallback-message" );
				div.setAttribute( "class", "amcharts-export-fallback" );
				_this.setup.chart.containerDiv.appendChild( div );

				// UPDATE MENU
				_this.createMenu( [ {
					"class": "export-main export-close",
					label: "Done",
					click: function() {
						_this.createMenu( _this.config.menu );
						_this.setup.chart.containerDiv.removeChild( div );
					}
				} ] );

				// ERROR
			} else {
				throw new Error( "Unable to create file. Ensure saveAs (FileSaver.js) is supported." );
			}
			return data;
		},

		/**
		 * Generates script, links tags and places them into the document's head
		 * In case of reload it replaces the node to force the download
		 */
		loadResource: function( src, addons ) {
			var i1, exist, node, item, check, type;
			var url = src.indexOf( "//" ) != -1 ? src : [ _this.libs.path, src ].join( "" );

			function callback() {
				if ( addons ) {
					for ( i1 = 0; i1 < addons.length; i1++ ) {
						_this.loadResource( addons[ i1 ] );
					}
				}
			}

			if ( src.indexOf( ".js" ) != -1 ) {
				node = document.createElement( "script" );
				node.setAttribute( "type", "text/javascript" );
				node.setAttribute( "src", url );
				if ( _this.libs.async ) {
					node.setAttribute( "async", "" );
				}

			} else if ( src.indexOf( ".css" ) != -1 ) {
				node = document.createElement( "link" );
				node.setAttribute( "type", "text/css" );
				node.setAttribute( "rel", "stylesheet" );
				node.setAttribute( "href", url );
			}

			for ( i1 = 0; i1 < document.head.childNodes.length; i1++ ) {
				item = document.head.childNodes[ i1 ];
				check = item ? ( item.src || item.href ) : false;
				type = item ? item.tagName : false;

				if ( item && check && check.indexOf( src ) != -1 ) {
					if ( _this.libs.reload ) {
						document.head.removeChild( item );
					}
					exist = true;
					break;
				}
			}

			if ( !exist || _this.libs.reload ) {
				node.addEventListener( "load", callback );
				document.head.appendChild( node );
			}

		},

		/**
		 * Walker to generate the script,link tags
		 */
		loadDependencies: function() {
			var i1, i2;
			if ( _this.libs.autoLoad ) {
				for ( i1 = 0; i1 < _this.libs.resources.length; i1++ ) {
					if ( _this.libs.resources[ i1 ] instanceof Object ) {
						for ( i2 in _this.libs.resources[ i1 ] ) {
							_this.loadResource( i2, _this.libs.resources[ i1 ][ i2 ] );
						}
					} else {
						_this.loadResource( _this.libs.resources[ i1 ] );
					}
				}
			}
		},

		/**
		 * Converts string to number
		 */
		pxToNumber: function( attr ) {
			return Number( String( attr ).replace( "px", "" ) ) || 0;
		},

		/**
		 * Converts number to string
		 */
		numberToPx: function( attr ) {
			return String( attr ) + "px";
		},

		/**
		 * Recursive method to merge the given objects together
		 * Overwrite flag replaces the value instead to crawl through
		 */
		deepMerge: function( a, b, overwrite ) {
			var i1, v, type = b instanceof Array ? "array" : "object";

			for ( i1 in b ) {
				// PREVENT METHODS
				if ( type == "array" && isNaN( i1 ) ) {
					continue;
				}

				v = b[ i1 ];

				// NEW
				if ( a[ i1 ] == undefined || overwrite ) {
					if ( v instanceof Array ) {
						a[ i1 ] = new Array();
					} else if ( v instanceof Function ) {
						a[ i1 ] = new Function();
					} else if ( v instanceof Date ) {
						a[ i1 ] = new Date();
					} else if ( v instanceof Object ) {
						a[ i1 ] = new Object();
					} else if ( v instanceof Number ) {
						a[ i1 ] = new Number();
					} else if ( v instanceof String ) {
						a[ i1 ] = new String();
					}
				}

				if ( !( v instanceof Function || v instanceof Date || _this.isElement( v ) ) && ( v instanceof Object || v instanceof Array ) ) {
					_this.deepMerge( a[ i1 ], v, overwrite );
				} else {
					if ( a instanceof Array && !overwrite ) {
						a.push( v );
					} else {
						a[ i1 ] = v;
					}
				}
			}
			return a;
		},

		/**
		 * Checks if given argument is a valid node
		 */
		isElement: function( thingy ) {
			return thingy instanceof Object && thingy && thingy.nodeType === 1;
		},

		/**
		 * Checks if given source is within the current origin
		 */
		isTainted: function( source ) {
			var origin = String( window.location.origin || window.location.protocol + "//" + window.location.hostname + ( window.location.port ? ':' + window.location.port : '' ) );

			// CHECK IF TAINTED
			if (
				source &&
				source.indexOf( "//" ) != -1 &&
				source.indexOf( origin.replace( /.*:/, "" ) ) == -1
			) {
				return true;
			}
			return false;
		},

		/**
		 * Recursive method which crawls upwards to gather the request attribute
		 */
		gatherAttribute: function( elm, attr, limit, lvl ) {
			var value, lvl = lvl ? lvl : 0,
				limit = limit ? limit : 3;
			if ( elm ) {
				value = elm.getAttribute( attr );

				if ( !value && lvl < limit ) {
					return _this.gatherAttribute( elm.parentNode, attr, limit, lvl + 1 );
				}
			}
			return value
		},

		/**
		 * Collects the clip-paths and patterns
		 */
		gatherElements: function( group, cfg, images ) {
			var i1, i2;
			for ( i1 = 0; i1 < group.children.length; i1++ ) {
				var childNode = group.children[ i1 ];

				// CLIPPATH
				if ( childNode.tagName == "clipPath" ) {
					for ( i2 = 0; i2 < childNode.childNodes.length; i2++ ) {
						childNode.childNodes[ i2 ].setAttribute( "fill", "transparent" );
					}
					group.clippings[ childNode.id ] = childNode;

					// PATTERN
				} else if ( childNode.tagName == "pattern" ) {
					var props = {
						node: childNode,
						source: childNode.getAttribute( "xlink:href" ),
						width: Number( childNode.getAttribute( "width" ) ),
						height: Number( childNode.getAttribute( "height" ) ),
						repeat: "repeat"
					}

					// GATHER BACKGROUND COLOR
					for ( i2 = 0; i2 < childNode.childNodes.length; i2++ ) {
						if ( childNode.childNodes[ i2 ].tagName == "rect" ) {
							props.fill = childNode.childNodes[ i2 ].getAttribute( "fill" );
						}
					}

					// TAINTED
					if ( cfg.removeImages && _this.isTainted( props.source ) ) {
						group.patterns[ childNode.id ] = props.fill ? props.fill : "transparent";
					} else {
						images.included++;

						// LOAD IMAGE MANUALLY; TO RERENDER THE CANVAS
						fabric.Image.fromURL( props.source, ( function( props ) {
							return function( img ) {
								images.loaded++;

								var patternSourceCanvas = new fabric.StaticCanvas( undefined, {
									backgroundColor: props.fill
								} );
								patternSourceCanvas.add( img );

								var pattern = new fabric.Pattern( {
									source: function() {
										patternSourceCanvas.setDimensions( {
											width: props.width,
											height: props.height
										} );
										return patternSourceCanvas.getElement();
									},
									repeat: 'repeat'
								} );

								group.patterns[ props.node.id ] = pattern;
							}
						} )( props ) );
					}

					// IMAGES
				} else if ( childNode.tagName == "image" ) {
					images.included++;

					// LOAD IMAGE MANUALLY; TO RERENDER THE CANVAS
					fabric.Image.fromURL( childNode.getAttribute( "xlink:href" ), function( img ) {
						images.loaded++;
					} );
				}
			}
			return group;
		},

		/**
		 * Method to capture the current state of the chart
		 */
		capture: function( options, callback ) {
			var i1;
			var cfg = _this.deepMerge( _this.deepMerge( {}, _this.config.fabric ), options || {} );
			var groups = [];
			var offset = {
				x: 0,
				y: 0,
				width: _this.setup.chart.divRealWidth,
				height: _this.setup.chart.divRealHeight
			};
			var images = {
				loaded: 0,
				included: 0
			}

			// GATHER SVGS
			var svgs = _this.setup.chart.containerDiv.getElementsByTagName( "svg" );
			for ( i1 = 0; i1 < svgs.length; i1++ ) {
				var group = {
					svg: svgs[ i1 ],
					parent: svgs[ i1 ].parentNode,
					children: svgs[ i1 ].getElementsByTagName( "*" ),
					offset: {
						x: 0,
						y: 0
					},
					patterns: {},
					clippings: {}
				}

				// GATHER ELEMENTS
				group = _this.gatherElements( group, cfg, images );

				// APPEND GROUP
				groups.push( group );
			}

			// GATHER EXTERNAL LEGEND
			if ( _this.config.legend && _this.setup.chart.legend && _this.setup.chart.legend.position == "outside" ) {
				var group = {
					svg: _this.setup.chart.legend.container.container,
					parent: _this.setup.chart.legend.container.container.parentNode,
					children: _this.setup.chart.legend.container.container.getElementsByTagName( "*" ),
					offset: {
						x: 0,
						y: 0
					},
					legend: {
						type: [ "top", "left" ].indexOf( _this.config.legend.position ) != -1 ? "unshift" : "push",
						position: _this.config.legend.position,
						width: _this.config.legend.width ? _this.config.legend.width : _this.setup.chart.legend.container.width,
						height: _this.config.legend.height ? _this.config.legend.height : _this.setup.chart.legend.container.height
					},
					patterns: {},
					clippings: {}
				}

				// ADAPT CANVAS DIMENSIONS
				if ( [ "left", "right" ].indexOf( group.legend.position ) != -1 ) {
					offset.width += group.legend.width;
					offset.height = group.legend.height > offset.height ? group.legend.height : offset.height;
				} else if ( [ "top", "bottom" ].indexOf( group.legend.position ) != -1 ) {
					offset.height += group.legend.height;
				}

				// GATHER ELEMENTS
				group = _this.gatherElements( group, cfg, images );

				// PRE/APPEND SVG
				groups[ group.legend.type ]( group );
			}

			// CLEAR IF EXIST
			_this.drawing.enabled = cfg.isDrawingMode = ( cfg.drawing && cfg.drawing.enabled ) ? true : cfg.action == "draw";

			if ( !_this.setup.wrapper ) {
				_this.setup.wrapper = document.createElement( "div" );
				_this.setup.wrapper.setAttribute( "class", _this.setup.chart.classNamePrefix + "-export-canvas" );
				_this.setup.chart.containerDiv.appendChild( _this.setup.wrapper );
			} else {
				_this.setup.wrapper.innerHTML = "";
			}

			// STOCK CHART
			if ( _this.setup.chart.type == "stock" ) {
				var padding = {
					top: 0,
					right: 0,
					bottom: 0,
					left: 0
				}
				if ( _this.setup.chart.leftContainer ) {
					offset.width -= _this.setup.chart.leftContainer.offsetWidth;
					padding.left = _this.setup.chart.leftContainer.offsetWidth + ( _this.setup.chart.panelsSettings.panelSpacing * 2 );
				}
				if ( _this.setup.chart.rightContainer ) {
					offset.width -= _this.setup.chart.rightContainer.offsetWidth;
					padding.right = _this.setup.chart.rightContainer.offsetWidth + ( _this.setup.chart.panelsSettings.panelSpacing * 2 );
				}
				if ( _this.setup.chart.periodSelector && [ "top", "bottom" ].indexOf( _this.setup.chart.periodSelector.position ) != -1 ) {
					offset.height -= _this.setup.chart.periodSelector.offsetHeight + _this.setup.chart.panelsSettings.panelSpacing;
					padding[ _this.setup.chart.periodSelector.position ] += _this.setup.chart.periodSelector.offsetHeight + _this.setup.chart.panelsSettings.panelSpacing;
				}
				if ( _this.setup.chart.dataSetSelector && [ "top", "bottom" ].indexOf( _this.setup.chart.dataSetSelector.position ) != -1 ) {
					offset.height -= _this.setup.chart.dataSetSelector.offsetHeight;
					padding[ _this.setup.chart.dataSetSelector.position ] += _this.setup.chart.dataSetSelector.offsetHeight;
				}
				// APPLY OFFSET ON WRAPPER
				_this.setup.wrapper.style.paddingTop = _this.numberToPx( padding.top );
				_this.setup.wrapper.style.paddingRight = _this.numberToPx( padding.right );
				_this.setup.wrapper.style.paddingBottom = _this.numberToPx( padding.bottom );
				_this.setup.wrapper.style.paddingLeft = _this.numberToPx( padding.left );
			}

			// CREATE CANVAS
			_this.setup.canvas = document.createElement( "canvas" );
			_this.setup.wrapper.appendChild( _this.setup.canvas );
			_this.setup.fabric = new fabric.Canvas( _this.setup.canvas, _this.deepMerge( {
				width: offset.width,
				height: offset.height
			}, cfg ) );

			// REAPPLY FOR SOME REASON
			_this.deepMerge( _this.setup.fabric, cfg );

			// OBSERVE OBJECT CREATION
			_this.setup.fabric.on( "object:added", function( e ) {
				var item = e.target;
				var state = JSON.stringify( item.originalState );
				if ( item.selectable && !item.known ) {
					_this.drawing.undos.push( {
						action: "added",
						target: item,
						options: state
					} );
					_this.drawing.redos = [];
				}
			} );

			// OBSERVE OBJECT MODIFICATIONS
			_this.setup.fabric.on( "object:modified", function( e ) {
				var item = e.target;
				var state = JSON.stringify( item.saveState().originalState );
				if ( item.selectable ) {
					_this.drawing.undos.push( {
						action: "modified",
						target: item,
						options: state
					} );
				}
			} );

			// DRAWING
			if ( _this.drawing.enabled ) {
				_this.setup.wrapper.setAttribute( "class", _this.setup.chart.classNamePrefix + "-export-canvas active" );
				_this.setup.wrapper.style.backgroundColor = cfg.backgroundColor;
			} else {
				_this.setup.wrapper.setAttribute( "class", _this.setup.chart.classNamePrefix + "-export-canvas" );
			}

			for ( i1 = 0; i1 < groups.length; i1++ ) {
				var group = groups[ i1 ];

				// GATHER POSITION
				if ( group.parent.style.top || group.parent.style.left ) {
					group.offset.y = _this.pxToNumber( group.parent.style.top );
					group.offset.x = _this.pxToNumber( group.parent.style.left );
				} else {
					// EXTERNAL LEGEND
					if ( group.legend ) {
						if ( group.legend.position == "left" ) {
							offset.x += group.legend.width;
						} else if ( group.legend.position == "right" ) {
							group.offset.x += offset.width - group.legend.width;
						} else if ( group.legend.position == "top" ) {
							offset.y += group.legend.height;
						} else if ( group.legend.position == "bottom" ) {
							group.offset.y += offset.height - group.legend.height; // OFFSET.Y
						}

						// NORMAL
					} else {
						group.offset.x = offset.x;
						group.offset.y = offset.y;
						offset.y += _this.pxToNumber( group.parent.style.height );
					}

					// PANEL
					if ( group.parent && ( group.parent.getAttribute( "class" ) || "" ).split( " " ).indexOf( "amChartsLegend" ) != -1 ) {
						offset.y += _this.pxToNumber( group.parent.parentNode.parentNode.style.marginTop );
						group.offset.y += _this.pxToNumber( group.parent.parentNode.parentNode.style.marginTop );
					}
				}

				// BEFORE CAPTURING
				_this.handleCallback( cfg.beforeCapture, cfg );

				// ADD TO CANVAS
				fabric.parseSVGDocument( group.svg, ( function( group ) {
					return function( objects, options ) {
						var i1;
						var g = fabric.util.groupSVGElements( objects, options );
						var tmp = {
							top: group.offset.y,
							left: group.offset.x,
							selectable: false
						};

						for ( i1 = 0; i1 < g.paths.length; i1++ ) {

							// OPACITY; TODO: DISTINGUISH OPACITY TYPES
							if ( g.paths[ i1 ] ) {

								// CHECK ORIGIN; REMOVE TAINTED
								if ( cfg.removeImages && _this.isTainted( g.paths[ i1 ][ "xlink:href" ] ) ) {
									g.paths.splice( i1, 1 );
									continue;
								}

								// SET OPACITY
								if ( g.paths[ i1 ].fill instanceof Object ) {

									// MISINTERPRETATION OF FABRIC
									if ( g.paths[ i1 ].fill.type == "radial" ) {
										g.paths[ i1 ].fill.coords.r2 = g.paths[ i1 ].fill.coords.r1 * -1;
										g.paths[ i1 ].fill.coords.r1 = 0;
									}

									g.paths[ i1 ].set( {
										opacity: g.paths[ i1 ].fillOpacity
									} );

									// PATTERN; TODO: DISTINGUISH OPACITY TYPES
								} else if ( String( g.paths[ i1 ].fill ).slice( 0, 3 ) == "url" ) {
									var PID = g.paths[ i1 ].fill.slice( 5, -1 );
									if ( group.patterns && group.patterns[ PID ] ) {
										g.paths[ i1 ].set( {
											fill: group.patterns[ PID ],
											opacity: g.paths[ i1 ].fillOpacity
										} );
									}
								}

								// CLIPPATH;
								if ( String( g.paths[ i1 ].clipPath ).slice( 0, 3 ) == "url" ) {
									var PID = g.paths[ i1 ].clipPath.slice( 5, -1 );

									if ( group.clippings[ PID ] ) {
										var mask = group.clippings[ PID ].childNodes[ 0 ];
										var transform = g.paths[ i1 ].svg.getAttribute( "transform" ) || "translate(0,0)";

										transform = transform.slice( 10, -1 ).split( "," );

										g.paths[ i1 ].set( {
											clipTo: ( function( mask, transform ) {
												return function( ctx ) {
													var width = Number( mask.getAttribute( "width" ) || "0" );
													var height = Number( mask.getAttribute( "height" ) || "0" );
													var x = Number( mask.getAttribute( "x" ) || "0" );
													var y = Number( mask.getAttribute( "y" ) || "0" );

													ctx.rect( Number( transform[ 0 ] ) * -1 + x, Number( transform[ 1 ] ) * -1 + y, width, height );
												}
											} )( mask, transform )
										} );
									}
								}

								// TODO; WAIT FOR TSPAN SUPPORT FROM FABRICJS SIDE
								if ( g.paths[ i1 ].originalBBox ) {
									var bb = g.paths[ i1 ].originalBBox;
									if ( g.paths[ i1 ].textAlign == "left" ) {
										g.paths[ i1 ].set( {
											left: bb.left + ( g.paths[ i1 ].width / 2 )
										} );
									} else {
										g.paths[ i1 ].set( {
											left: bb.left - ( g.paths[ i1 ].width / 2 )
										} );
									}
								}
							}
						}

						g.set( tmp );

						_this.setup.fabric.add( g );

						// ADD BALLOONS
						if ( group.svg.parentNode && group.svg.parentNode.getElementsByTagName ) {
							var balloons = group.svg.parentNode.getElementsByClassName( _this.setup.chart.classNamePrefix + "-balloon-div" );
							for ( i1 = 0; i1 < balloons.length; i1++ ) {
								if ( cfg.balloonFunction instanceof Function ) {
									cfg.balloonFunction.apply( _this, [ balloons[ i1 ], group ] );
								} else {
									var parent = balloons[ i1 ];
									var text = parent.childNodes[ 0 ];
									var label = new fabric.Text( text.innerText || text.innerHTML, {
										fontSize: _this.pxToNumber( text.style.fontSize ),
										fontFamily: text.style.fontFamily,
										fill: text.style.color,
										top: _this.pxToNumber( parent.style.top ) + group.offset.y,
										left: _this.pxToNumber( parent.style.left ) + group.offset.x,
										selectable: false
									} );

									_this.setup.fabric.add( label );
								}
							}
						}
						if ( group.svg.nextSibling && group.svg.nextSibling.tagName == "A" ) {
							var label = new fabric.Text( group.svg.nextSibling.innerText || group.svg.nextSibling.innerHTML, {
								fontSize: _this.pxToNumber( group.svg.nextSibling.style.fontSize ),
								fontFamily: group.svg.nextSibling.style.fontFamily,
								fill: group.svg.nextSibling.style.color,
								top: _this.pxToNumber( group.svg.nextSibling.style.top ) + group.offset.y,
								left: _this.pxToNumber( group.svg.nextSibling.style.left ) + group.offset.x
							} );
							_this.setup.fabric.add( label );
						}

						groups.pop();

						// TRIGGER CALLBACK WITH SAFETY DELAY
						if ( !groups.length ) {
							var timer = setInterval( function() {
								if ( images.loaded == images.included ) {
									clearTimeout( timer );
									_this.handleCallback( cfg.afterCapture, cfg );
									_this.setup.fabric.renderAll();
									_this.handleCallback( callback, cfg );
								}
							}, AmCharts.updateRate );
						}
					}

					// IDENTIFY ELEMENTS THROUGH CLASSNAMES
				} )( group ), function( svg, obj ) {
					var i1;
					var className = _this.gatherAttribute( svg, "class" );
					var visibility = _this.gatherAttribute( svg, "visibility" );
					var clipPath = _this.gatherAttribute( svg, "clip-path" );

					obj.className = className;
					obj.clipPath = clipPath;
					obj.svg = svg;

					// TODO; WAIT FOR TSPAN SUPPORT FROM FABRICJS SIDE
					if ( svg.tagName == "text" && svg.childNodes.length > 1 ) {
						var lines = [];
						var textAnchor = svg.getAttribute( "text-anchor" ) || "left";
						var anchorMap = {
							"start": "left",
							"middle": "center",
							"end": "right"
						}

						for ( i1 = 0; i1 < svg.childNodes.length; i1++ ) {
							lines.push( svg.childNodes[ i1 ].textContent );
						}

						if ( obj.className == _this.setup.chart.classNamePrefix + "-label" ) {
							obj.originalBBox = obj.getBoundingRect()
						}
						obj.set( {
							top: obj.top + ( ( obj.height / 2 ) * ( lines.length - 1 ) ),
							text: lines.join( "\n" ),
							textAlign: anchorMap[ textAnchor ]
						} );
					}

					// HIDE HIDDEN ELEMENTS; TODO: FIND A BETTER WAY TO HANDLE THAT
					if ( visibility == "hidden" ) {
						obj.opacity = 0;

						// WALKTHROUGH ELEMENTS
					} else {

						// TRANSPORT FILL/STROKE OPACITY
						var attrs = [ "fill", "stroke" ];
						for ( i1 = 0; i1 < attrs.length; i1++ ) {
							var attr = attrs[ i1 ]
							var attrVal = String( svg.getAttribute( attr ) || "" );
							var attrOpacity = Number( svg.getAttribute( attr + "-opacity" ) || "1" );
							var attrRGBA = fabric.Color.fromHex( attrVal ).getSource();

							// EXCEPTION
							if ( obj.className == _this.setup.chart.classNamePrefix + "-guide-fill" && !attrVal ) {
								attrOpacity = 0;
								attrRGBA = fabric.Color.fromHex( "#000000" ).getSource();
							}

							if ( attrRGBA ) {
								attrRGBA.pop();
								attrRGBA.push( attrOpacity )
								obj[ attr ] = "rgba(" + attrRGBA.join() + ")";
								obj[ _this.capitalize( attr + "-opacity" ) ] = attrOpacity;
							}
						}
					}

					// REVIVER
					_this.handleCallback(cfg.reviver, obj, svg);
				} );
			}
		},

		/**
		 * Returns the current canvas
		 */
		toCanvas: function( options, callback ) {
			var cfg = _this.deepMerge( {
				// NUFFIN
			}, options || {} );
			var data = _this.setup.canvas;

			_this.handleCallback( callback, data );

			return data;
		},

		/**
		 * Returns an image; by default PNG
		 */
		toImage: function( options, callback ) {
			var cfg = _this.deepMerge( {
				format: "png",
				quality: 1,
				multiplier: 1
			}, options || {} );
			var data = cfg.data;
			var img = document.createElement( "img" );

			if ( !cfg.data ) {
				if ( cfg.lossless || cfg.format == "svg" ) {
					data = _this.toSVG( _this.deepMerge( cfg, {
						getBase64: true
					} ) );
				} else {
					data = _this.setup.fabric.toDataURL( cfg );
				}
			}

			img.setAttribute( "src", data );

			_this.handleCallback( callback, img );

			return img;
		},

		/**
		 * Generates a blob instance image; returns base64 datastring
		 */
		toBlob: function( options, callback ) {
			var cfg = _this.deepMerge( {
				data: "empty",
				type: "text/plain"
			}, options || {} );
			var isBase64 = /^data:.+;base64,(.*)$/.exec( cfg.data );

			// GATHER BODY
			if ( isBase64 ) {
				cfg.data = isBase64[ 0 ];
				cfg.type = cfg.data.slice( 5, cfg.data.indexOf( "," ) - 7 );
				cfg.data = _this.toByteArray( {
					data: cfg.data.slice( cfg.data.indexOf( "," ) + 1, cfg.data.length )
				} );
			}

			if ( cfg.getByteArray ) {
				data = cfg.data;
			} else {
				data = new Blob( [ cfg.data ], {
					type: cfg.type
				} );
			}

			_this.handleCallback( callback, data );

			return data;
		},

		/**
		 * Generates JPG image; returns base64 datastring
		 */
		toJPG: function( options, callback ) {
			var cfg = _this.deepMerge( {
				format: "jpeg",
				quality: 1,
				multiplier: 1
			}, options || {} );
			var data = _this.setup.fabric.toDataURL( cfg );

			_this.handleCallback( callback, data );

			return data;
		},

		/**
		 * Generates PNG image; returns base64 datastring
		 */
		toPNG: function( options, callback ) {
			var cfg = _this.deepMerge( {
				format: "png",
				quality: 1,
				multiplier: 1
			}, options || {} );
			var data = _this.setup.fabric.toDataURL( cfg );

			_this.handleCallback( callback, data );

			return data;
		},

		/**
		 * Generates SVG image; returns base64 datastring
		 */
		toSVG: function( options, callback ) {
			var cfg = _this.deepMerge( {
				// NOTHING IN HERE
			}, options || {} );
			var data = _this.setup.fabric.toSVG( cfg );

			if ( cfg.getBase64 ) {
				data = "data:image/svg+xml;base64," + btoa( data );
			}

			_this.handleCallback( callback, data );

			return data;
		},

		/**
		 * Generates PDF; returns base64 datastring
		 */
		toPDF: function( options, callback ) {
			var cfg = _this.deepMerge( _this.deepMerge( {
				multiplier: 2
			}, _this.config.pdfMake ), options || {}, true );
			cfg.images.reference = _this.toPNG( cfg );
			var data = new pdfMake.createPdf( cfg );

			if ( callback ) {
				data.getDataUrl( ( function( callback ) {
					return function() {
						callback.apply( _this, arguments );
					}
				} )( callback ) );
			}

			return data;
		},

		/**
		 * Generates an image; hides all elements on page to trigger native print method
		 */
		toPRINT: function( options, callback ) {
			var i1;
			var cfg = _this.deepMerge( {
				delay: 1,
				lossless: false
			}, options || {} );
			var data = _this.toImage( cfg );
			var states = [];
			var items = document.body.childNodes;

			data.setAttribute( "style", "width: 100%; max-height: 100%;" );

			for ( i1 = 0; i1 < items.length; i1++ ) {
				if ( _this.isElement( items[ i1 ] ) ) {
					states[ i1 ] = items[ i1 ].style.display;
					items[ i1 ].style.display = "none";
				}
			}

			document.body.appendChild( data );
			window.print();

			setTimeout( function() {
				for ( i1 = 0; i1 < items.length; i1++ ) {
					if ( _this.isElement( items[ i1 ] ) ) {
						items[ i1 ].style.display = states[ i1 ];
					}
				}
				document.body.removeChild( data );
				_this.handleCallback( callback, data );
			}, cfg.delay );

			return data;
		},

		/**
		 * Generates JSON string
		 */
		toJSON: function( options, callback ) {
			var cfg = _this.deepMerge( {
				data: _this.getChartData()
			}, options || {}, true );
			var data = JSON.stringify( cfg.data, undefined, "\t" );

			_this.handleCallback( callback, data );

			return data;
		},

		/**
		 * Generates CSV string
		 */
		toCSV: function( options, callback ) {
			var row, col;
			var cfg = _this.deepMerge( {
				data: _this.getChartData(),
				delimiter: ",",
				quotes: true,
				escape: true,
				dateFields: [],
				dateFormat: _this.setup.chart.dataDateFormat || "YYYY-MM-DD"
			}, options || {}, true );
			var data = "";
			var cols = [];
			var buffer = [];

			if ( _this.setup.chart.categoryAxis && _this.setup.chart.categoryAxis.parseDates && _this.setup.chart.categoryField ) {
				cfg.dateFields.push( _this.setup.chart.categoryField );
			}

			function enchant( value, column ) {

				// STRING
				if ( typeof value === "string" ) {
					value = value;

					// DATE FORMAT
				} else if ( column && cfg.dateFormat && value instanceof Date && cfg.dateFields.indexOf( column ) != -1 ) {
					value = AmCharts.formatDate( value, cfg.dateFormat );
				}

				// WRAP IN QUOTES				
				if ( typeof value === "string" ) {
					if ( cfg.escape ) {
						value = value.replace( '"', '""' );
					}
					if ( cfg.quotes ) {
						value = [ '"', value, '"' ].join( "" );
					}
				}

				return value;
			}

			// HEADER
			for ( value in cfg.data[ 0 ] ) {
				buffer.push( enchant( value ) );
				cols.push( value );
			}
			data += buffer.join( cfg.delimiter ) + "\n";

			// BODY
			for ( row in cfg.data ) {
				buffer = [];
				if ( !isNaN( row ) ) {
					for ( col in cols ) {
						if ( !isNaN( col ) ) {
							var column = cols[ col ];
							var value = cfg.data[ row ][ column ];

							buffer.push( enchant( value, column ) );
						}
					}
					data += buffer.join( cfg.delimiter ) + "\n";
				}
			}

			_this.handleCallback( callback, data );

			return data;
		},

		/**
		 * Generates excel sheet; returns base64 datastring
		 */
		toXLSX: function( options, callback ) {
			var cfg = _this.deepMerge( {
				data: _this.getChartData(),
				name: "amCharts",
				withHeader: true
			}, options || {}, true );
			var data = "";
			var wb = {
				SheetNames: [],
				Sheets: {}
			}

			function datenum( v, date1904 ) {
				if ( date1904 ) v += 1462;
				var epoch = Date.parse( v );
				return ( epoch - new Date( Date.UTC( 1899, 11, 30 ) ) ) / ( 24 * 60 * 60 * 1000 );
			}

			function sheet_from_array_of_arrays( data, opts ) {
				var ws = {};
				var range = {
					s: {
						c: 10000000,
						r: 10000000
					},
					e: {
						c: 0,
						r: 0
					}
				};
				for ( var R = 0; R != data.length; ++R ) {
					for ( var C = 0; C != data[ R ].length; ++C ) {
						if ( range.s.r > R ) range.s.r = R;
						if ( range.s.c > C ) range.s.c = C;
						if ( range.e.r < R ) range.e.r = R;
						if ( range.e.c < C ) range.e.c = C;
						var cell = {
							v: data[ R ][ C ]
						};
						if ( cell.v == null ) continue;
						var cell_ref = XLSX.utils.encode_cell( {
							c: C,
							r: R
						} );

						if ( typeof cell.v === "number" ) cell.t = "n";
						else if ( typeof cell.v === "boolean" ) cell.t = "b";
						else if ( cell.v instanceof Date ) {
							cell.t = "n";
							cell.z = XLSX.SSF._table[ 14 ];
							cell.v = datenum( cell.v );
						} else cell.t = "s";

						ws[ cell_ref ] = cell;
					}
				}
				if ( range.s.c < 10000000 ) ws[ "!ref" ] = XLSX.utils.encode_range( range );
				return ws;
			}

			wb.SheetNames.push( cfg.name );
			wb.Sheets[ cfg.name ] = sheet_from_array_of_arrays( _this.toArray( cfg ) );

			data = XLSX.write( wb, {
				bookType: "xlsx",
				bookSST: true,
				type: "base64"
			} );

			data = "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64," + data;

			_this.handleCallback( callback, data );

			return data;
		},

		/**
		 * Generates an array of arrays
		 */
		toArray: function( options, callback ) {
			var row, col;
			var cfg = _this.deepMerge( {
				data: _this.getChartData(),
				dateFields: [],
				dateFormat: false,
				withHeader: false
			}, options || {}, true );
			var data = [];
			var cols = [];

			// HEADER
			if ( cfg.withHeader ) {
				for ( col in cfg.data[ 0 ] ) {
					cols.push( col );
				}
				data.push( cols );
			}

			// BODY
			for ( row in cfg.data ) {
				var buffer = [];
				if ( !isNaN( row ) ) {
					for ( col in cols ) {
						var col = cols[ col ];
						var value = cfg.data[ row ][ col ] || "";

						if ( cfg.dateFormat && value instanceof Date && cfg.dateFields.indexOf( col ) != -1 ) {
							value = AmCharts.formatDate( value, cfg.dateFormat );
						} else {
							value = String( value )
						}

						buffer.push( value );
					}
					data.push( buffer );
				}
			}

			_this.handleCallback( callback, data );

			return data;
		},

		/**
		 * Generates byte array with given base64 datastring; returns byte array
		 */
		toByteArray: function( options, callback ) {
			var cfg = _this.deepMerge( {
				// NUFFIN
			}, options || {} );
			var Arr = ( typeof Uint8Array !== 'undefined' ) ? Uint8Array : Array
			var PLUS = '+'.charCodeAt( 0 )
			var SLASH = '/'.charCodeAt( 0 )
			var NUMBER = '0'.charCodeAt( 0 )
			var LOWER = 'a'.charCodeAt( 0 )
			var UPPER = 'A'.charCodeAt( 0 )
			var data = b64ToByteArray( cfg.data );

			function decode( elt ) {
				var code = elt.charCodeAt( 0 )
				if ( code === PLUS )
					return 62 // '+'
				if ( code === SLASH )
					return 63 // '/'
				if ( code < NUMBER )
					return -1 //no match
				if ( code < NUMBER + 10 )
					return code - NUMBER + 26 + 26
				if ( code < UPPER + 26 )
					return code - UPPER
				if ( code < LOWER + 26 )
					return code - LOWER + 26
			}

			function b64ToByteArray( b64 ) {
				var i, j, l, tmp, placeHolders, arr

				if ( b64.length % 4 > 0 ) {
					throw new Error( 'Invalid string. Length must be a multiple of 4' )
				}

				// THE NUMBER OF EQUAL SIGNS (PLACE HOLDERS)
				// IF THERE ARE TWO PLACEHOLDERS, THAN THE TWO CHARACTERS BEFORE IT
				// REPRESENT ONE BYTE
				// IF THERE IS ONLY ONE, THEN THE THREE CHARACTERS BEFORE IT REPRESENT 2 BYTES
				// THIS IS JUST A CHEAP HACK TO NOT DO INDEXOF TWICE
				var len = b64.length
				placeHolders = '=' === b64.charAt( len - 2 ) ? 2 : '=' === b64.charAt( len - 1 ) ? 1 : 0

				// BASE64 IS 4/3 + UP TO TWO CHARACTERS OF THE ORIGINAL DATA
				arr = new Arr( b64.length * 3 / 4 - placeHolders )

				// IF THERE ARE PLACEHOLDERS, ONLY GET UP TO THE LAST COMPLETE 4 CHARS
				l = placeHolders > 0 ? b64.length - 4 : b64.length

				var L = 0

				function push( v ) {
					arr[ L++ ] = v
				}

				for ( i = 0, j = 0; i < l; i += 4, j += 3 ) {
					tmp = ( decode( b64.charAt( i ) ) << 18 ) | ( decode( b64.charAt( i + 1 ) ) << 12 ) | ( decode( b64.charAt( i + 2 ) ) << 6 ) | decode( b64.charAt( i + 3 ) )
					push( ( tmp & 0xFF0000 ) >> 16 )
					push( ( tmp & 0xFF00 ) >> 8 )
					push( tmp & 0xFF )
				}

				if ( placeHolders === 2 ) {
					tmp = ( decode( b64.charAt( i ) ) << 2 ) | ( decode( b64.charAt( i + 1 ) ) >> 4 )
					push( tmp & 0xFF )
				} else if ( placeHolders === 1 ) {
					tmp = ( decode( b64.charAt( i ) ) << 10 ) | ( decode( b64.charAt( i + 1 ) ) << 4 ) | ( decode( b64.charAt( i + 2 ) ) >> 2 )
					push( ( tmp >> 8 ) & 0xFF )
					push( tmp & 0xFF )
				}

				return arr
			}

			_this.handleCallback( callback, data );

			return data;
		},

		/**
		 * Callback handler; injects additional arguments to callback
		 */
		handleCallback: function( callback ) {
			var i1, data = Array();
			if ( callback && callback instanceof Function ) {
				for ( i1 = 0; i1 < arguments.length; i1++ ) {
					if ( i1 > 0 ) {
						data.push(arguments[i1]);
					}
				}
				callback.apply( _this, data );
			}
		},

		/**
		 * Gathers chart data according to its type
		 */
		getChartData: function() {
			var i1, i2, i3;
			var data = [];

			if ( _this.setup.chart.type == "stock" ) {
				data = _this.setup.chart.mainDataSet.dataProvider;
			} else if ( _this.setup.chart.type == "gantt" ) {
				var segmentsField = _this.setup.chart.segmentsField;
				for ( i1 = 0; i1 < _this.setup.chart.dataProvider.length; i1++ ) {
					if ( _this.setup.chart.dataProvider[ i1 ][ segmentsField ] ) {
						for ( i2 = 0; i2 < _this.setup.chart.dataProvider[ i1 ][ segmentsField ].length; i2++ ) {
							data.push( _this.setup.chart.dataProvider[ i1 ][ segmentsField ][ i2 ] )
						}
					}
				}
			} else {
				data = _this.setup.chart.dataProvider;
			}

			return data;
		},

		/**
		 * Prettifies string
		 */
		capitalize: function( string ) {
			return string.charAt( 0 ).toUpperCase() + string.slice( 1 ).toLowerCase();
		},

		/**
		 * Generates export menu; returns UL node
		 */
		createMenu: function( list, container ) {
			var div;

			function buildList( list, container ) {
				var i1, ul = document.createElement( "ul" );
				for ( i1 = 0; i1 < list.length; i1++ ) {
					var item = typeof list[ i1 ] === "string" ? {
						format: list[ i1 ]
					} : list[ i1 ];
					var li = document.createElement( "li" );
					var a = document.createElement( "a" );
					var img = document.createElement( "img" );
					var span = document.createElement( "span" );
					var action = String( item.action ? item.action : item.format ).toLowerCase();

					item.format = String( item.format ).toUpperCase();

					// MERGE WITH GIVEN FORMAT
					if ( _this.config.formats[ item.format ] ) {
						item = _this.deepMerge( {
							label: item.icon ? "" : item.format,
							format: item.format,
							mimeType: _this.config.formats[ item.format ].mimeType,
							extension: _this.config.formats[ item.format ].extension,
							capture: _this.config.formats[ item.format ].capture,
							action: _this.config.action,
							fileName: _this.config.fileName
						}, item );
					} else if ( !item.menu && !item.items ) {
						item.label = item.label ? item.label : _this.capitalize( action );
					}

					// FILTER; TOGGLE FLAG
					if ( [ "CSV", "JSON", "XLSX" ].indexOf( item.format ) != -1 && [ "map", "gauge" ].indexOf( _this.setup.chart.type ) != -1 ) {
						continue;

						// BLOB EXCEPTION
					} else if ( !_this.setup.hasBlob && item.format != "UNDEFINED" ) {
						if ( item.mimeType && item.mimeType.split( "/" )[ 0 ] != "image" && item.mimeType != "text/plain" ) {
							continue;
						}
					}

					// ADD CLICK HANDLER
					if ( !item.click && !item.menu && !item.items ) {

						// DRAWING METHODS
						if ( _this.drawing.actions.indexOf( action ) != -1 ) {
							item.action = action;
							item.click = ( function( item ) {
								return function() {
									this.drawing[ item.action ]();
								}
							} )( item );

							// DRAWING
						} else if ( _this.drawing.enabled ) {
							item.click = ( function( item ) {
								return function() {
									this[ "to" + item.format ]( item, function( data ) {
										this.drawing.done();
										if ( item.action != "print" && item.format != "PRINT" ) {
											this.download( data, item.mimeType, [ item.fileName, item.extension ].join( "." ) );
										}
									} );
								}
							} )( item );

							// REGULAR
						} else if ( item.format != "UNDEFINED" ) {
							item.click = ( function( item ) {
								return function() {
									if ( item.capture || ( item.action == "print" || item.format == "PRINT" ) ) {
										this.capture( item, function() {
											this[ "to" + item.format ]( item, function( data ) {
												if ( item.action == "download" ) {
													this.download( data, item.mimeType, [ item.fileName, item.extension ].join( "." ) );
												}
											} );
										} )

									} else if ( this[ "to" + item.format ] ) {
										this[ "to" + item.format ]( item, function( data ) {
											this.download( data, item.mimeType, [ item.fileName, item.extension ].join( "." ) );
										} );
									} else {
										throw new Error( 'Invalid format. Could not determine output type.' );
									}
								}
							} )( item );
						}
						// DRAWING
					} else if ( item.action == "draw" ) {
						item.click = ( function( item ) {
							return function() {
								this.capture( item, function() {
									this.createMenu( item.menu );
								} );
							}
						} )( item );
					}

					// ADD LINK ATTR
					a.setAttribute( "href", "#" );
					a.addEventListener( "click", ( function( callback, item ) {
						return function( e ) {
							e.preventDefault();

							// DELAYED
							item.delay = item.delay ? item.delay : _this.config.delay;
							if ( item.delay ) {
								_this.delay( item, callback );
								return;
							}
							callback.apply( _this, arguments );
						}
					} )( item.click || function( e ) {
						e.preventDefault();
					}, item ) );
					li.appendChild( a );

					// ADD LABEL
					span.innerHTML = item.label;

					// APPEND ITEMS
					if ( item[ "class" ] ) {
						li.className = item[ "class" ];
					}
					if ( item.icon ) {
						img.setAttribute( "src", ( item.icon.slice( 0, 10 ).indexOf( "//" ) == -1 ? chart.pathToImages : "" ) + item.icon );
						a.appendChild( img );
					}
					if ( item.label ) {
						a.appendChild( span );
					}
					if ( item.title ) {
						a.setAttribute( "title", item.title );
					}

					// CALLBACK; REVIVER FOR MENU ITEMS
					if ( _this.config.menuReviver ) {
						li = _this.config.menuReviver.apply( _this, [ item, li ] );
					}

					// ADD SUBLIST; JUST WITH ENTRIES
					if ( ( item.menu || item.items ) && item.action != "draw" ) {
						if ( buildList( item.menu || item.items, li ).childNodes.length ) {
							ul.appendChild( li );
						}
					} else {
						ul.appendChild( li );
					}
				}

				// JUST ADD THOSE WITH ENTRIES
				return container.appendChild( ul );
			}

			// DETERMINE CONTAINER
			if ( !container ) {
				if ( typeof _this.config.divId == "string" ) {
					_this.config.divId = container = document.getElementById( _this.config.divId );
				} else if ( _this.isElement( _this.config.divId ) ) {
					container = _this.config.divId;
				} else {
					container = _this.setup.chart.containerDiv;
				}
			}

			// CREATE / RESET MENU CONTAINER
			if ( _this.isElement( _this.setup.menu ) ) {
				_this.setup.menu.innerHTML = "";
			} else {
				_this.setup.menu = document.createElement( "div" );
			}
			_this.setup.menu.setAttribute( "class", _this.setup.chart.classNamePrefix + "-export-menu " + _this.setup.chart.classNamePrefix + "-export-menu-" + _this.config.position + " amExportButton" );

			// CALLBACK; REPLACES THE MENU WALKER
			if ( _this.config.menuWalker ) {
				buildList = _this.config.menuWalker;
			}
			buildList.apply( this, [ list, _this.setup.menu ] );

			container.appendChild( _this.setup.menu );

			return _this.setup.menu;
		},

		/**
		 * Method to trigger the callback delayed
		 */
		delay: function( options, callback ) {
			var cfg = _this.deepMerge( {
				delay: 3,
				precision: 2
			}, options || {} );
			var t1, t2, start = Number( new Date() );
			var menu = _this.createMenu( [ {
				label: _this.i18l( "capturing.delayed.menu.label" ).replace( "{{duration}}", AmCharts.toFixed( cfg.delay, cfg.precision ) ),
				title: _this.i18l( "capturing.delayed.menu.title" ),
				"class": "export-delayed-capturing",
				click: function() {
					clearTimeout( t1 );
					clearTimeout( t2 );
					_this.createMenu( _this.config.menu );
				}
			} ] );
			var label = menu.getElementsByTagName( "a" )[ 0 ];

			// MENU UPDATE
			t1 = setInterval( function() {
				var diff = cfg.delay - ( Number( new Date() ) - start ) / 1000;
				if ( diff <= 0 ) {
					clearTimeout( t1 );
					if ( cfg.action != "draw" ) {
						_this.createMenu( _this.config.menu );
					}
				} else if ( label ) {
					label.innerHTML = _this.i18l( "capturing.delayed.menu.label" ).replace( "{{duration}}", AmCharts.toFixed( diff, 2 ) );
				}
			}, 10 );

			// CALLBACK
			t2 = setTimeout( function() {
				callback.apply( _this, arguments );
			}, cfg.delay * 1000 );
		},

		/**
		 * Migration method to support old export setup
		 */
		migrateSetup: function( chart ) {
			if ( chart.amExport || chart.exportConfig ) {
				var config = _this.deepMerge( {
					enabled: true,
					migrated: true,
					libs: {
						autoLoad: false
					}
				}, _this.deepMerge( _this.defaults, {
					menu: []
				}, true ) );

				function crawler( object ) {
					var key;
					for ( key in object ) {
						var value = object[ key ];

						if ( key.slice( 0, 6 ) == "export" && value ) {
							config.menu.push( key.slice( 6 ) );
						} else if ( key == "userCFG" ) {
							crawler( value );
						} else if ( key == "menuItems" ) {
							config.menu = value;
						} else if ( key == "libs" ) {
							config.libs = value;
						} else if ( typeof key == "string" ) {
							config[ key ] = value;
						}
					}
				}
				crawler( chart.amExport || chart.exportConfig );
				chart[ "export" ] = config;
			}
			return chart;
		},

		/**
		 * Initiate export instance; waits for chart container to place menu
		 */
		init: function() {
			clearTimeout( _this.timer );
			_this.timer = setInterval( function() {
				if ( _this.setup.chart.containerDiv ) {
					clearTimeout( _this.timer );
					_this.setup.chart.AmExport = _this;

					// WORK AROUND TO BYPASS FILESAVER CHECK TRYING TO OPEN THE BLOB URL IN SAFARI BROWSER
					window.safari = window.safari ? window.safari : {};

					_this.createMenu( _this.config.menu );
				}
			}, AmCharts.updateRate );
		}
	}

	// EXTEND DRAWING TO SUPPORT "CANCEL" MENU ACTION
	_this.drawing.cancel = _this.drawing.done;

	// MIRGRATE
	_this.setup.chart = _this.migrateSetup( chart );

	// ENABLED-I-O?
	if ( undefined === _this.setup.chart[ "export" ] || !_this.setup.chart[ "export" ].enabled ) {
		return;
	}

	// CHECK BLOB CONSTRUCTOR
	try {
		_this.setup.hasBlob = !!new Blob;
	} catch ( e ) {}

	// MERGE SETTINGS
	_this.deepMerge( _this.libs, _this.setup.chart[ "export" ].libs || {}, true );
	_this.deepMerge( _this.defaults.pdfMake, _this.setup.chart[ "export" ] );
	_this.deepMerge( _this.defaults.fabric, _this.setup.chart[ "export" ] );
	_this.config = _this.deepMerge( _this.defaults, _this.setup.chart[ "export" ], true );

	// SUPPORT IE ONLY IF WE'VE ACCESS TO THE HEAD
	if ( AmCharts.isIE && AmCharts.IEversion <= 9 ) {
		if ( !document.head || _this.config.fallback === false ) {
			return;
		}
	}

	// REPLACE CONFIG WITH INSTANCE; ENABLE ADDCLASSNAMES
	_this.setup.chart[ "export" ] = _this;
	_this.setup.chart.addClassNames = true;

	// LOAD DEPENDENCIES
	_this.loadDependencies( _this.libs.resources, _this.libs.reload );

	// INIT
	_this.init();

}, [ "pie", "serial", "xy", "funnel", "radar", "gauge", "stock", "map", "gantt" ] );