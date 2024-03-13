import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";

const Register= () => {
    const [name, setName] = useState([]);
    const [surName, setSurName] = useState([]);
    const [username, setUserName] = useState([]);
    const [email, setEmail] = useState([]);
    const [password, setPassword] = useState([]);
    const [phone, setPhone] = useState([]);
    const [role, setRole] = useState("user");

    const navigate = useNavigate();

    const submit =  () => {
        const formData = {
                name: name,
                surname: surName,
                username: username,
                email: email,
                password: password,
                phoneNumber: phone,
                role: role
        };
        
        const response = fetch('https://localhost:7050/api/User/Register',{
            method:'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData) 
        })
        .then(response => {
            if (response.ok) {
                navigate('/login');
            }
        }) 

        
    };
    
    return(
        <div>
            
                <div className="form-floating input-row">
                    <input className="form-control" placeholder="Name" required onChange={(e) => setName(e.target.value)}/>
                    <label >Name</label>
                </div>
                <div className="form-floating input-row">
                    <input className="form-control" placeholder="Prezime " required onChange={(e) => setSurName(e.target.value)}/>
                    <label >Surname</label>
                </div>
                <div className="form-floating input-row">
                    <input className="form-control" placeholder="KorisnickoIme" required onChange={(e) => setUserName(e.target.value)}/>
                    <label >Username</label>
                </div>
                <div className="form-floating input-row">
                    <input type="email" className="form-control" placeholder="n@example.com" required onChange={(e) => setEmail(e.target.value)}/>
                    <label >Email</label>
                </div>
                <div className="form-floating input-row">
                    <input type="password" className="form-control" placeholder="Lozinka" required onChange={(e) => setPassword(e.target.value)}/>
                    <label >Password</label>
                </div>
                <div className="form-floating input-row">
                    <input type="tel" className="form-control" placeholder="Telefon" required onChange={(e) => setPhone(e.target.value)}/>
                    <label >Phone</label>
                </div>
                <div style={{ padding: 10 }}>
                    <label ><input type="checkbox" placeholder="Admin" value={"admin"} onChange={(e) => setRole(e.target.value)} />Are you an admin?</label>   
                </div>
                <Link className="nav-link login-label" to={"/login"} >Already have an account?</Link>
                <button className="btn btn-primary w-100 py-2 mb-4" placeholder="SignUp" type="submit" onClick={submit}>Sign up</button>
            
        </div>
    );
}

export default Register;