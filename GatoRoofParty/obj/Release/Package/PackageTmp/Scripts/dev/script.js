
function VerifyCredentials()
{
    $("#Name_Err").empty();
    $("#Surname_Err").empty();
    $("#Email_Err").empty();
    $("#Date_of_Birth_Err").empty();
    $("#Error_Err").empty();

    if ($('input[name=checkfield]').prop('checked')) {
        var flags = [verifyName($.trim($('[name="namefield"]').val())), verifySurname($.trim($('[name="surnamefield"]').val())), verifyEmail($.trim($('[name="emailfield"]').val())), verifyAge($('[name="datefield"]').val())]
        var flag = true;

        //get all INVALID input field names
        for (var i = 0; i < flags.length; i++) {
            var v = flags[i].trim();

            if (v != "valid")//(v != null || v != "" || !v || v.length > 0)
            {
                var t = flags[i];
                var d = "#" + t.replace(/\ /g, '_') + "_Err";
                $(d).html(t.replace(/\_/g, ' ') + " is invalid!");
                flag = false;
            }
        }
    }
    else
    {
        $("#Error_Err").html("You must agree to the terms and conditions!");
        flag = false;
    }
        

    return flag;
}


//returns true if valid, false if NOT
function verifyEmail(email)
{
	var name = ""; //johndoe <-- @
	var domain = ""; //site  <-- .com
	var extension = ""; //.com
	
	if(email != null && email != "")
	{
		//get @ symbol, split name
		for(var i = 0; i < email.length; i++)
		{
			if(email[i] == "@")
			{
				name = email.substring(0, i);
				domain = email.substring(i + 1, email.length);
				break;
			}
		}
		
		//get first . symbol, split domain
		for(var i = 0; i < domain.length; i++)
		{
			if(domain[i] == ".")
			{
				extension = domain.substring(i + 1, domain.length)
				domain = domain.substring(0, i);
				break;
			}
		}
		
		if(isAlphanumeric(name) && isDomain(domain) && isExtension(extension) && email.length < 150)
			return "valid";
		else
			return "Email";
	}
	else
	{
		return "Email";
	}
}

function verifyName(name)
{
    if (name.length < 100)
        return isName(name);
    else
        return "Name";
}

function verifySurname(surname) {
    if (surname.length < 100)
        return isSurname(surname);
    else
        return "Surname";
}

function verifyAge(date)
{
	//first check for valid date sent
	if(isDate(date))
	{
		//now check user's age
		var today = new Date();
		var birthDate = new Date(date);
		var age = today.getFullYear() - birthDate.getFullYear();
		var m = today.getMonth() - birthDate.getMonth();
		if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
			age--;
		}
		
		if(age >= 18)
			return "valid";
		else
			return "Date of Birth";
	}
	else
		return "Date of Birth";
}


// re-usable methods
function isAlphanumeric(str)
{
	var code;
	var dotbefore = false;//if true, previous letter was a .
	var underbefore = false;//if true, previous latter was an _
	
	for (var i = 0; i < str.length; i++)
	{
		code = str.charCodeAt(i); //get code 
		//$('#name').append(str[i]);
		
		//NOT numeric, upper-alpha, lower-alpha, dot/period. Space = 32
		if( !(code > 47 && code < 58) && !(code > 64 && code < 91) && !(code > 96 && code < 123) && code != 46 && code != 95)
		{ 
			return false;
		}
		else if(code == 46)  //check it's not followed by another dot/period
		{	
			//if followed by a dot, or is at start/end
			if((i == 0 || i == str.length - 1) || dotbefore) 
				return false;
			else
				dotbefore = true; //set flag
		}
		else if(code == 95)
		{
		    if ((i == 0 || i == str.length - 1) || underbefore)
		        return false;
		    else
		        underbefore = true; //set flag
		}
		else
		{
			dotbefore = false; //reset
		}
	}
	return true;
}

function isDomain(str)
{
	var code;
	var dashbefore = false;//if true, previous letter was a -
	
	for (var i = 0; i < str.length; i++)
	{
		code = str.charCodeAt(i); //get code 
		//$('#domain').append(str[i]);
		
		//NOT numeric, upper-alpha, lower-alpha, dash
		if( !(code > 47 && code < 58) && !(code > 64 && code < 91) && !(code > 96 && code < 123) && code != 45 )
		{ 
			return false;
		}
		else if(code == 45)  //check it's not followed by another dash
		{	
			if(!dashbefore)//if no dash was found
			{
				if((i == 0 || i == str.length - 1)) 
					return false;
				else
					dashbefore = true; //set flag
			}
			else
				return false;
		}
	}
	return true;
}

function isExtension(str)
{
	var code;
	var dotbefore = false;//if true, previous letter was a .
	
	for (var i = 0; i < str.length; i++)
	{
		code = str.charCodeAt(i); //get code 
		//$('#extension').append(str[i]);

		//NOT numeric, upper-alpha, lower-alpha, dot/period
		if( !(code > 47 && code < 58) && !(code > 64 && code < 91) && !(code > 96 && code < 123) && code != 46 )
		{ 
			return false;
		}
		else if(code == 46)  //check it's not followed by another dot/period
		{	
			if(!dotbefore)//if no dot was found
			{
				if((i == 0 || i == str.length - 1)) 
					return false;
				else
					dotbefore = true; //set flag
			}
			else
				return false;
		}

	}
	return true;
}

function isDate(val)
{
    var d = new Date(val);
    return !isNaN(d.valueOf());
}

function restrict()
{
    if (event.keyCode == 32)
	{
        return false;
    }
}

function isName(str)
{
	//allow only one space and one dash
	var code;
	var dashbefore = false;//if true, previous letter was a -
	var spacebefore = false;//if true, space was used at least once already
	
	if (!str)
	    return "Name";

	for (var i = 0; i < str.length; i++)
	{
		code = str.charCodeAt(i); //get code 
		//$('#domain').append(str[i]);
		
		//NOT numeric, upper-alpha, lower-alpha, dash
		if( !(code > 47 && code < 58) && !(code > 64 && code < 91) && !(code > 96 && code < 123) && code != 45 && code != 32)
		{ 
			return "Name";
		}
		else if(code == 45)//check it's not followed by another dash
		{	
			if(!dashbefore)//if no dash was found
			{
				if((i == 0 || i == str.length - 1)) 
				    return "Name";
				else
					dashbefore = true; //set flag
			}
			else
			    return "Name";
		}
		else if(code == 32)//check if one space already used
		{
			if(!spacebefore)//if no space was found
			{
				if((i == 0 || i == str.length - 1)) 
				    return "Name";
				else
					spacebefore = true; //set flag
			}
			else
			    return "Name";
		}
	}
	return "valid";
}

function isSurname(str) {
    //allow only one space and one dash
    var code;
    var dashbefore = false;//if true, previous letter was a -
    var spacebefore = false;//if true, space was used at least once already

    if (!str)
        return "Surname";

    for (var i = 0; i < str.length; i++) {
        code = str.charCodeAt(i); //get code 
        //$('#domain').append(str[i]);

        //NOT numeric, upper-alpha, lower-alpha, dash
        if (!(code > 47 && code < 58) && !(code > 64 && code < 91) && !(code > 96 && code < 123) && code != 45 && code != 32) {
            return "Surname";
        }
        else if (code == 45)//check it's not followed by another dash
        {
            if (!dashbefore)//if no dash was found
            {
                if ((i == 0 || i == str.length - 1))
                    return "Surname";
                else
                    dashbefore = true; //set flag
            }
            else
                return "Surname";
        }
        else if (code == 32)//check if one space already used
        {
            if (!spacebefore)//if no space was found
            {
                if ((i == 0 || i == str.length - 1))
                    return "Surname";
                else
                    spacebefore = true; //set flag
            }
            else
                return "Surname";
        }
    }
    return "valid";
}
