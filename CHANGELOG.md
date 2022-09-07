# Changelog

The latest version of the package fork of Smooth.Foundations is available at:

https://https://github.com/FWCorey/smooth.foundations.package


## [2.0.3] - 2022-09-07
### Changed

- Converted to UnityPackage
- Updated documentation to match package recommended formatting

## [2.0.2] - 2014-05-06
### Changed

Factory: Converted out value methods to tuples.

Slinq: Added SkipAndReturn(), RemoveAndReturn() methods to facilitate chaining.

Slinq: RemoveAll() methods return a count.


## [2.0] - 2014-05-05
### Changed

Foundations is available under the MIT License.

Added Smooth.Slinq and Smooth.Compare.

Added InspectorKeyValuePair for facilitating inspector based dictionary management.

Converted API methods of Smooth.Compare that used null values to Option<T>.

Register KeyCode comparer on AOT platforms.


## [1.2.1] - 2014-05-01
### Changed

Disposable<T> is a struct on console platforms.

Updated documentation.


## [1.2] - 2014-04-28
### Changed

Static pools expose an instance property rather than individual pooling methods.

Added PoolWithInitializer<T, U> class.

Added console platforms to JIT check.

Disposable<T> is a struct on iOS to prevent compute_class_bitmap errors.


## [1.1] - 2014-04-25
### Changed

Updated documentation.


## [1.0] - 2014-04-25
### Changed

Initial Release.
