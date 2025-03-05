create table Cadenas
(
	ID_Cadena INTEGER not null
		primary key,
	ClaveCadena TEXT,
	RazonSocial TEXT,
	NombreComercial TEXT,
	ID_Giro INTEGER,
	ClaveGiro TEXT,
	DescripcionGiro TEXT
);

create table Promociones
(
	ID_Promocion INTEGER not null
		primary key,
	Descripcion TEXT,
	EsDestacada INTEGER,
	Restricciones TEXT,
	VigenciaInicio TEXT,
	VigenciaFin TEXT,
	ClavePromocion TEXT,
	OrderBanner INTEGER,
	OrdenPromoHome INTEGER,
	TituloPromocion TEXT,
	TipoDescuento TEXT,
	ClaveTipoCupon TEXT,
	EsFavorito NUMERIC,
	ID_Cadena INTEGER
		references Cadenas,
	ServerOrder REAL
);

create table Redes
(
	ID_RedCadena INTEGER not null
		primary key,
	Clave TEXT,
	Descripcion TEXT,
	Valor TEXT,
	ID_Cadena INTEGER
		references Cadenas
);

create table Sucursales
(
	id_sucursal INTEGER not null
		primary key,
	Clave TEXT,
	nombre TEXT,
	latitud REAL,
	longitud REAL,
	calle TEXT,
	colonia TEXT,
	ciudad TEXT,
	CveEstado TEXT,
	Estado TEXT,
	cp TEXT,
	telefono TEXT,
	ID_Cadena INTEGER
		references Cadenas
);

create table Tipo
(
	id_tipo INTEGER not null,
	key TEXT not null,
	description TEXT not null
);