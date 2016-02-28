uint getID( uint id  ){

  uint base = floor( id / 6 );
  uint tri  = id % 6;
  uint row = floor( base / _RibbonWidth );
  uint col = (base) % _RibbonWidth;

  uint rowU = (row + 1) % _RibbonLength;
  uint colU = (col + 1) % _RibbonWidth;

  uint rDoID = row * _RibbonWidth;
  uint rUpID = rowU * _RibbonWidth;
  uint cDoID = col;
  uint cUpID = colU;


  uint fID = 0;

  if( tri == 0 ){
    fID = rDoID + cDoID;
  }else if( tri == 1 ){
    fID = rUpID + cDoID;
  }else if( tri == 2 ){
    fID = rUpID + cUpID;
  }else if( tri == 3 ){
    fID = rDoID + cDoID;
  }else if( tri == 4 ){
    fID = rUpID + cUpID;
  }else if( tri == 5 ){
    fID = rDoID + cUpID;
  }else{
    fID = 0;
  }
  return fID;

}
            