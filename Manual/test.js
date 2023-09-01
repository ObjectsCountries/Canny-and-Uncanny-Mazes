var ch=document.getElementsByClassName("checkerboard");
var letters=["A","B","C","D","E","F","G","H"];
var st="";
for(let i=5;i<9;i+=1){
   	st=st+"    private Dictionary<string,List<string>> wallsMaze"+i+"x"+i+"=new Dictionary<string,List<string>>(){\n";
  for(let j=0;j<i;j+=1){
    for(let k=0;k<i;k+=1){
      st=st+"        {\""+letters[k]+(j+1)+"\",new List<string>(){\""+ch[i-5].children[j].children[k].className.replace(/ /g,"\",\"")+"\"}}";
      if(k!=i-1||j!=i-1)
        st+=",";
     	if(k==i-1&&j!=i-1)
        st+="\n";
      st+="\n";
    }
  }
  st=st+"    };\n";
}
console.log(st);