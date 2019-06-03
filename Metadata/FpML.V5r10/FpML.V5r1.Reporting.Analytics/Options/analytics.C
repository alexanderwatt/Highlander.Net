#include "math.h"
#include "malloc.h"
#include "stdio.h"
#include "dates.h"
#include "bonds.h"
#include "string.h"
#include"options.h"
#include "analytics.h"

static MCGrid mcgrid;
static LPMCTermStructure mctermstructures[ 100 ];


BOOLEAN QuantoForward( double forward_yield, double vol_fx, double vol_yield, double correlation, double time, double* result )
{
	if( forward_yield<0 || vol_fx<0.0 || vol_yield<0.0 || correlation<-1.0 || correlation>1.0 ) return FALSE;

	*result = forward_yield*exp( -correlation*vol_fx*vol_yield*time/365.0 );

	return TRUE;
}



BOOLEAN CummBivarNormDist( double a, double b, double corr, double* result )
{
	int i, j;
	double A[ 5 ] = { 0.24840615, 0.39233107, 0.21141819, 0.033246660, 0.00082485334 };
	double B[ 5 ] = { 0.10024215, 0.48281397, 1.0609498, 1.7797294, 2.6697604 };
	double ad, bd, corr1, corr2, delta, total, result1, result2;

	if( a<=0.0 && b<=0.0 && corr<=0.0 ){
		ad = a/sqrt( 2.0*(1-corr*corr) );
		bd = b/sqrt( 2.0*(1-corr*corr) );

		total = 0.0;
		for( i=0; i<5; i++ ){
			for( j=0; j<5; j++ ){
				total+=( A[ i ]*A[ j ]*exp( ad*(2.0*B[ i ]-ad) + bd*(2.0*B[ j ]-bd )+2.0*corr*(B[ i ]-ad)*(B[ j ]-bd) ) );
			}
		}
		total *= sqrt( 1.0-corr*corr )/_PI;
		*result = total;

		return TRUE;
	}

	if( a<=0.0 && b>=0.0 && corr>=0.0 ){
			CummBivarNormDist( a, -b, -corr, result );
			total = CummNormDist( a ) - (*result);

			*result = total;
			return TRUE;
	}

	if( a>=0.0 && b<=0.0 && corr>=0.0 ){
			CummBivarNormDist( -a, b, -corr, result );
			total = CummNormDist( b ) - (*result);

			*result = total;
			return TRUE;
	}

	if( a>=0.0 && b>=0.0 && corr<=0.0 ){
			CummBivarNormDist( -a, -b, corr, result );
			total = CummNormDist( a ) + CummNormDist( b ) - 1.0 + (*result);

			*result = total;
			return TRUE;
	}

	if( a*b*corr>0.0 ){
			corr1 = (corr*a-b)*SGN(a)/sqrt(a*a-2*corr*a*b+b*b);
			corr2 = (corr*b-a)*SGN(b)/sqrt(a*a-2*corr*a*b+b*b);
			delta = (1.0-SGN(a)*SGN(b))/4.0;

			CummBivarNormDist( a, 0.0, corr1, &result1 );
			CummBivarNormDist( b, 0.0, corr2, &result2 );

			total =  result1+ result2 - delta;
			*result = total;

			return TRUE;
	}

	return TRUE;
}



BOOLEAN PricetoYieldVol( double pricevol, double bpv, double price, double yield, double* yieldvol )
{
	double conv_factor;

	if( pricevol<0.0 || bpv<0 || price<=0.0 || yield<=0.0 ) return FALSE;

	conv_factor = -log( (price+bpv)/price )/log( (yield-0.0001)/yield );

	*yieldvol = pricevol/conv_factor;

	return TRUE;
}


BOOLEAN YieldtoPriceVol( double yieldvol, double bpv, double price, double yield, double* pricevol )
{
	double conv_factor;

	if( yieldvol<0.0 || bpv<0.0 || yield<0.0 ) return FALSE;

	conv_factor = -log( (price+bpv)/price )/log( (yield-0.0001)/yield );

	*pricevol = yieldvol*conv_factor;

	return TRUE;
}


BOOLEAN ConvexityAdjustment( double coupon,int tenor, int freq, double yield, double volatility, double days, double* adj )
{
	double price, duration, bpv, convexity;

	if( coupon<0.0 ) return FALSE;
	if( tenor<0 ) return FALSE;
	if( freq<0 ) return FALSE;
	if( yield<0 ) return FALSE;
	if( volatility<0 ) return FALSE;

	if( !BondFeatures( coupon, tenor, freq, yield, 0, &price, &duration, &bpv, &convexity ) ) return FALSE;

	*adj = 0.5*yield*yield*volatility*volatility*days/365.0*convexity/(duration/100.0);

	return TRUE;
}


BOOLEAN DelayofPaymentAdjustment( double yield, double lagtenor, double R, double correlation, double vol_y, double vol_R, double days, double* adj )
{
	if( R*lagtenor==-1.0 ) return FALSE;

	*adj = (yield*lagtenor*R*correlation*vol_y*vol_R*days/365.0)/(1.0+R*lagtenor);

	return TRUE;
}

/*
BOOLEAN CMSAdjustment( double coupon, int tenor, int freq, double yield, double volatility, double days, double* adj )
{
	double price, duration, bpv, convexity;

	if( coupon<0.0 ) return FALSE;
	if( tenor<0 ) return FALSE;
	if( freq<0 ) return FALSE;
	if( yield<0 ) return FALSE;
	if( volatility<0 ) return FALSE;

	if( !BondFeatures( coupon, tenor, freq, yield, 0, &price, &duration, &bpv, &convexity ) ) return FALSE;

	*adj = 0.5*yield*yield*volatility*volatility*days/365.0*convexity/(duration/100.0);

	return TRUE;
}
*/		

BOOLEAN BasketVolatility( double weights[50], int num_weights, double volatilities[50], int num_volatilities, double correlations[50][50], double* basket_volatility )
{
	int i, j;
	double temp[ 50 ], b_vol;

	if( num_weights>50 || num_volatilities>50 ) return FALSE;
	if( num_weights<1 || num_volatilities<1 ) return FALSE;
	if( num_weights!=num_volatilities ) return FALSE;

	for( i=0; i<num_weights; i++ )
		if( correlations[ i ][ i ] !=1.0 ) return FALSE;

	for( i=0; i<num_weights; i++ ){
		temp[ i ] = 0.0;
		for( j=0; j<num_weights; j++ ){
			temp[ i ] = temp[ i ]+weights[ j ]*volatilities[ j ]*correlations[ j ][ i ];
		}
	}

	b_vol = 0.0;
	for( i=0; i<num_weights; i++ ){
			 b_vol += ( temp[ i ]*weights[ i ]*volatilities[ i ] );
	}

	if( b_vol<0.0 ) return FALSE;

	*basket_volatility = sqrt( b_vol );
	
	return TRUE;
}



BOOLEAN OptionsHorizonBE( OPTION* portfolio, int num_options, double time_horizon, int direction, double tolerance, double *be_price, BOOLEAN* good_price )
{
	int          i, loop_count;
	double currentprice, basevalue, currentvalue, currentdelta;

	*good_price = FALSE;

	basevalue = RevalueOptions( portfolio, num_options, 0 );
	for( i=0; i<num_options; i++ ) (portfolio+i)->days_to_expiry = (portfolio+i)->days_to_expiry+time_horizon;
	currentprice = portfolio[ 0 ].current;

	//This checks for a breakeven to the upside in price
	currentvalue = RevalueOptions( portfolio, num_options, 0 );
	currentdelta = RevalueOptions( portfolio, num_options, 1 );
	loop_count = 0;
	while( fabs( (currentvalue-basevalue) )>tolerance && loop_count<20 ){
		for( i=0; i<num_options; i++ ){
			switch( direction ){
				case DIR_UP			:  (portfolio+i)->current = (portfolio+i)->current-(currentvalue-basevalue)/currentdelta;
													  break;
				case DIR_DOWN	:  (portfolio+i)->current = (portfolio+i)->current+(currentvalue-basevalue)/currentdelta;
												 	  break;
			}
		}
		currentvalue = RevalueOptions( portfolio, num_options, 0 );
		currentdelta = RevalueOptions( portfolio, num_options, 1 );
		loop_count++;
	}
	if( loop_count<20 ){
		*be_price = portfolio[0].current;
		*good_price = TRUE;
	}

	return TRUE;
}
	


double RevalueOptions( OPTION* options, int num_options, int return_type )
{
	int i;
	double return_value;
	double value = 0.0;

	for( i=0; i<num_options; i++ ){
		if( BS( (options+i)->callput, (options+i)->current, (options+i)->strike, (options+i)->volatility, (options+i)->days_to_expiry, (options+i)->rf_rate, return_type, &return_value ) )
			value += return_value;
		else
			return 0.0;
	}

	return value;
}





								   
								   
								   
BOOLEAN LoadMonteCarloData( char* filename )
{
		FILE* fp=NULL;
		char  buffer[ 4096 ];
		char* token;
		int	     count;

		if( mcgrid.data_numbers!=NULL )
			return TRUE;

		if( (fp = fopen( filename, "rt" ))==NULL )
			return FALSE;

		if( mcgrid.data_numbers==NULL )
			mcgrid.data_numbers = (double*)calloc( MAX_NUM_ASSETS*MAX_SIMULATION_PATHS, sizeof(double));
		if( mcgrid.data_numbers==NULL ) 
			return FALSE;

		count = 0;
		fgets( buffer, 4096, fp );
		while( !feof(fp) ){
			token = strtok( buffer, "," );
			while( token!=NULL ){
				if( count<MAX_NUM_ASSETS*MAX_SIMULATION_PATHS ){
					*( mcgrid.data_numbers+count ) = atof( token );
					count++;
				}else
					break;
				token = strtok( NULL, "," );
			}
			fgets( buffer, 4096, fp );
		}
		if( fp!=NULL ) 	fclose( fp );

		return TRUE;
}




BOOLEAN AllocateTermStructureSpace( int *term_structure_id )
{
	int i;

	*term_structure_id = -1;
	for( i=0; i<MAX_TERM_STRUCTURES; i++ )
		if( mctermstructures[ i ]==NULL ) break;

	if( i==MAX_TERM_STRUCTURES )
		return FALSE;

	mctermstructures[ i ] = (MCTermStructure*)calloc( 1, sizeof(MCTermStructure) );
	if( mctermstructures[ i ] == NULL )
		return FALSE;

	mctermstructures[ i ]->cd.matrix = (double*)calloc( MAX_NUM_ASSETS*MAX_NUM_ASSETS, sizeof(double ));
	if( mctermstructures[ i ]->cd.matrix == NULL )
		return FALSE;

	*term_structure_id = i;

	return TRUE;
}



BOOLEAN PopulateTermStructure( int term_structure_id, double forwards[ MAX_NUM_ASSETS ], double volatilities[ MAX_NUM_ASSETS ], double days[ MAX_NUM_ASSETS ] )
{
	int        i, j, num_assets;
	double correlations[ MAX_NUM_ASSETS ][ MAX_NUM_ASSETS ];

	if( !( term_structure_id>=0 && term_structure_id<MAX_NUM_ASSETS ) ) return FALSE;
	if( mctermstructures[ term_structure_id ]==NULL ) return FALSE;

	for( j=0; j<MAX_NUM_ASSETS && days[ j ]>0; j++ ){
		mctermstructures[ term_structure_id ]->days[ j ] = days[ j ];
		mctermstructures[ term_structure_id ]->forwards[ j ] = forwards[ j ];
		mctermstructures[ term_structure_id ]->volatilities[ j ] = volatilities[ j ];
		mctermstructures[ term_structure_id ]->moment1[ j ] = 0.0;
		mctermstructures[ term_structure_id ]->moment2[ j ] = 1.0;
	}

	num_assets = j-1;
	for( i=0; i<num_assets; i++ ){
		for( j=0; j<num_assets; j++ ){
			correlations[ i ][ j ] = (volatilities[ i ]*sqrt( days[ i ]/365.0 ) )/( volatilities[ j ]*sqrt( days[ j ]/365.0 ) );
		}
	}

	mctermstructures[ term_structure_id ]->cd.num_rows = num_assets;
	mctermstructures[ term_structure_id ]->cd.num_cols = num_assets;
	CholeskyDecomposition( correlations, mctermstructures[ term_structure_id ]->cd.matrix, num_assets, num_assets );

	return TRUE;
}


BOOLEAN CalculateMomentAdjustments( int term_structure_id )
{
	int	         i, j;
	double path[ MAX_NUM_ASSETS ];
	double moment1[ MAX_NUM_ASSETS ], moment2[ MAX_NUM_ASSETS ];
	LPMCTermStructure ts;

	if( !( term_structure_id>=0 && term_structure_id<MAX_NUM_ASSETS ) ) return FALSE;
	if( mctermstructures[ term_structure_id ]==NULL ) return FALSE;
	
	ts = mctermstructures[ term_structure_id ];

	// Clear the moment accumulators to neutral values
	for( j=0; j<MAX_NUM_ASSETS; j++ ){
		ts->moment1[ j ] = moment1[ j ] = 0.0;
		ts->moment2[ j ] = moment2[ j ] = 1.0;
	}

	// Go through each of the simulation paths and get the path
	for( i=0; i<MAX_SIMULATION_PATHS*2; i++ ){
		if( !GetSimulation( term_structure_id, i, path ) ) break;
		// Go through each of the valid assets and accumulate moment1 (mean) and moment2 (stdev )
		for( j=0; j<MAX_NUM_ASSETS && path[ j ]>=0.0; j++ ){
			if( i==0 ){
				moment1[ j ] = path[ j ];
				moment2[ j ] = pow( log( path[ j ]/ts->forwards[ j ] ), 2 );
			}else{
				moment1[ j ] += path[ j ];
				moment2[ j ] += pow( log( path[ j ]/ts->forwards[ j ] ), 2 );
			}
		}
	}

	// Go through each of the assets moments and convert to arithmetic mean and stdev of the log returns
	for( j=0; j<MAX_NUM_ASSETS && moment1[ j ]>=0.0 && moment2[ j ]>=0.0; j++ ){
		ts->moment1[ j ] = ts->forwards[ j ] - moment1[ j ]/(MAX_SIMULATION_PATHS*2);
		ts->moment2[ j ] = sqrt( moment2[ j ]/(MAX_SIMULATION_PATHS*2-1) )/sqrt( ts->days[ j ]/365.0 );
		ts->moment2[ j ] = ts->volatilities[ j ]/ts->moment2[ j ];
	}

	return TRUE;
}




BOOLEAN GetSimulation( int term_structure_id, int simulation_number, double path[ 91 ] )
{
	int        i, j;
	double forward_0, volatility_0, days_0;
	double forward_1, volatility_1, days_1;
	double r, days_n, vol_n, sobol_num, cd_element;
	double random_num[ MAX_NUM_ASSETS ];
	BOOLEAN antithetic;
	LPMCTermStructure ts;
	
	if( term_structure_id<0 || term_structure_id>=MAX_NUM_ASSETS ) return FALSE;
	if( simulation_number<0 || simulation_number>=MAX_SIMULATION_PATHS*2 ) return FALSE;

	antithetic=FALSE;
	if( simulation_number>=MAX_SIMULATION_PATHS ){
		simulation_number = simulation_number-MAX_SIMULATION_PATHS;
		antithetic = TRUE;
	}

	//Setup a quick pointer to a term structure
	ts = mctermstructures[ term_structure_id ];

	//Clean the simulation path
	for( i=0; i<MAX_NUM_ASSETS; i++ ) path[ i ] = -99.0;

	//Apply the Cholesky Decomposition to create the correlated random vector
	for( i=0; i<ts->cd.num_rows; i++ ){
		random_num[ i ]= mcgrid.data_numbers[ simulation_number*91+i ]*(antithetic? -1 : 1 );
//		random_num[ i ] = 0.0;
//		for( j=0; j<ts->cd.num_cols; j++ ){
//			sobol_num = mcgrid.data_numbers[ simulation_number*91+j ]*(antithetic? -1 : 1 );
//			cd_element = ts->cd.matrix[ i*MAX_NUM_ASSETS+j ];
//			random_num[ i ] += ( sobol_num*cd_element );
//		}
	}

	// Apply Initial Bootstrapping
	forward_0        = ts->forwards[ 0 ];
	volatility_0        = ts->volatilities[ 0 ];
	days_0             = ts->days[ 0 ];
	path[ 0 ]     = forward_0*exp( -0.5*volatility_0*volatility_0*days_0/365.0 + volatility_0*sqrt(days_0/365.0)*random_num[ 0 ] );

	// Apply subsequent Bootstrapping
	for( i=1; i<MAX_NUM_ASSETS && ts->days[ i ]>0.0; i++ ){
		forward_1 = ts->forwards[ i ];
		volatility_1 = ts->volatilities[ i ];
		days_1      = ts->days[ i ];

		days_n      = days_1-days_0;
		r				   = log( forward_1/forward_0 )*365.0/days_n;
		vol_n		   = sqrt( ( volatility_1*volatility_1*days_1 - volatility_0*volatility_0*days_0 )/days_n );

		path[ i ]      = path[ i-1 ]*exp( (r-0.5*vol_n*vol_n)*days_n/365.0 +vol_n*sqrt(days_n/365.0)*random_num[ i ] );

		forward_0 = forward_1;
		volatility_0 = volatility_1;
		days_0      = days_1;
	}

	// Centre the simulations to match moment1 and moment2
	for( i=0; i<MAX_NUM_ASSETS && path[ i ]>0; i++ ){
		path[ i ] = ts->forwards[ i ]*exp( log(path[ i ]/ts->forwards[ i ]) * ts->moment2[ i ] ) ;
		path[ i ] += ts->moment1[ i ];
	}

	return TRUE;
}


void DestroyMCGrid()
{
	if( mcgrid.data_numbers==NULL ) return;

	free( (void*)mcgrid.data_numbers );
}


void DestroyMCTermStructures()
{
	int i;

	for( i=0; i<MAX_TERM_STRUCTURES; i++ ){
		if( mctermstructures[ i ]==NULL ) return;
		free( (void*)mctermstructures[ i ]->cd.matrix );
		free( (void*)mctermstructures[ i ] );
	}
}


BOOLEAN _2DLookup( double* _luarray, double _lurows, double _lucols, int num_rows, int num_cols, double* luvalue )
{
	int        col_index = 0, row_index = 0;
	double r1, r2, c1, c2;
	double v1, v2, v3, v4;
	double iv1, iv2;

	if( num_rows==0 || num_cols==0 ) return FALSE;
	if( _luarray==NULL ) return FALSE;

	if( _luarray[ 1 ]>=_lucols ) col_index = 1;
	if( col_index==0 && _luarray[ num_cols-1 ]<=_lucols ) col_index = num_cols-1;
	if( col_index==0 )
		for( col_index = 1; col_index<num_cols-1; col_index++ ){
			if( _luarray[ col_index ]<=_lucols && _luarray[ col_index+1 ]>=_lucols )
				break;
		}

	if( _luarray[ num_cols ]>=_lurows ) row_index = 1;
	if( row_index==0 && _luarray[ (num_rows-1)*num_cols+1 ]<=_lucols ) row_index = num_rows-1;
	if( row_index==0 )
		for( row_index = 1; row_index<num_rows-1; row_index++ ){
			if( _luarray[ row_index*num_cols ]<=_lurows && _luarray[ (row_index+1)*num_cols ]>=_lurows )
				break;
		}
	
	r1 = _luarray[ row_index*num_cols ];
	r2 = _luarray[ (row_index+1)*num_cols ];

	v1 = _luarray[ row_index*num_cols+col_index ];
	v2 = _luarray[ (row_index+1)*num_cols+col_index ];
	v3 = _luarray[ row_index*num_cols+col_index+1 ];
	v4 = _luarray[ (row_index+1)*num_cols+col_index+1 ];

	c1 = _luarray[ col_index ];
	c2 = _luarray[ col_index+1 ];

	//Simple two step Linear Interpolation
	iv1 = (_lurows-r1)/(r2-r1)*(v2-v1)+v1;
	iv2 = (_lurows-r1)/(r2-r1)*(v4-v3)+v3;

	*luvalue = (_lucols-c1)/(c2-c1)*(iv2-iv1)+iv1;

	return TRUE;
}



BOOLEAN CholeskyDecomposition( double correlations[91][91], double cholesky[91][91], int num_rows, int num_cols )
{
	int i, j, k;
	double x;

	for( i=0; i<91 && i<num_rows; i++ ){
		for( j=0; j<91 && j<num_cols; j++ ){
			cholesky[ i ][ j ]=0.0;
		}
	}

	for( i=0; i<91 && i<num_rows; i++ ){
		for( j=i; j<91 && j<num_cols; j++ ){
			x=correlations[ i ][ j ];
			for( k=0; k<i; k++ ){
				x = x - cholesky[ i ][ k ]*cholesky[ j ][ k ];
			}
			if( j==i )
				cholesky[ i ][ i ] = sqrt( x );
			else
				cholesky[ j ][ i ] = x/cholesky[ i ][ i ];
		}
	}

	return TRUE;
}


/*
BOOLEAN IntrinsicCallVolatility( JDATES dates[ MAX_COUPONDATES ], double forwards[ MAX_COUPONDATES ], double start_vol, double* intrinsic_vol )
{
	int        i, num_dates;
	double option_value, option_expiry_value, delta_hedge_value;
	double vol, tolerance;

	for( num_dates=0; num_dates<MAX_COUPONDATES && dates[ num_dates ]!=BAD_JDATE; num_dates++ )
	
	strike = forwards[ num_dates-1 ];
	vol = start_vol;
	option_value = 	BS('C', forward_rates[ i-1 ], strike, start_vol, dates[ i-1 ]-dates[ 0 ], 0.0, 0, &opt_price );
	delta_hedge_value = option_value;
	if( forward_rates[ 0 ]>strike ) option_expiry_value = (forward_rates[  0 ]-strike ); option_expiry_value = 0.0;

	while( fabs(portfolio_value-option_value)>tolerance )
	{
		delta_hedge_value = 0.0;
		for( i=num_dates-1; i>0; i-- ){
			BS('C', forward_rates[ i-1 ], strike, vol, dates[ i-1 ]-dates[ 0 ], 0.0, 1, &opt_delta );
			delta_hedge_value -= (forward_rates[ i-1 ]-forward_rates[ i ])*opt_delta; 
		}
		portfolio_value = option_value+delta_hedge_value+option_expiry
		vol *= option_value/portfolio_value;
	}
}
*/





