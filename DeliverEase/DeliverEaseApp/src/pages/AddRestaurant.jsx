import { useState } from "react";
import { useNavigate } from "react-router-dom";

const AddRestaurant = () => {
    const [name, setName] = useState('');
    const [address, setAddress] = useState('');
    const [rating, setRating] = useState(0);
    const [meals, setMeals] = useState([]);

    const [mealName, setMealName] = useState('');
    const [mealDescription, setMealDescription] = useState('');
    const [mealPrice, setMealPrice] = useState();

    const [nameError, setNameError] = useState('');
    const [addressError, setAddressError] = useState('');

    const navigate = useNavigate();


    const submit = async (e) => {
        e.preventDefault();

        if (!name) {
            setNameError('Please fill Restaurant name.');
            return;
        }

        if (!address) {
            setAddressError('Please fill Restaurant address.');
            return;
        }

        const newRestaurant = {
            name: name,
            address: address,
            rating: rating,
            meals: meals
        };

        const response = await fetch('https://localhost:7050/api/Restaurant/CreateRestaurant', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(newRestaurant),
        });

        navigate('/account')
    }

    const handleOnClick = (e) => {
        e.preventDefault();
        
        if(mealName !== '' && mealDescription !== '' && mealPrice !== 0){
            const newMeal = {
                name: mealName,
                description: mealDescription,
                price: mealPrice
            };

            setMeals((prevMeals) => [...prevMeals, newMeal]);

            setMealName('');
            setMealDescription('');
            setMealPrice(0);
        }
        else {
            setError('Please fill in all fields for the meal.');
        }
    }
    
    return(
        <div>
            <form>
                <div className="form-floating input-row">
                    <input className="form-control" placeholder="Name" required onChange={e => setName(e.target.value)}/>
                    <label >Name</label>
                    {nameError && <div className="error">{nameError}</div>}
                </div>
                <div className="form-floating input-row">
                    <input className="form-control" placeholder="Address" required onChange={e => setAddress(e.target.value)}/>
                    <label >Address</label>
                    {addressError && <div className="error">{addressError}</div>}
                </div>
                <button placeholder="Create Restaurant" onClick={submit}>Create Restaurant</button>
                {meals.map((meal, id) => {
                    return(
                        <div key={id}>
                            <div>
                                {meal.name}
                            </div>
                            <div>
                                {meal.description}
                            </div>
                            <div>
                                {meal.price}
                            </div>
                        </div>
                    )
                })}
                <div>
                    Add another meal
                    <div>
                        <div className="form-floating input-row">
                            <input className="form-control" placeholder="Ime jela" value={mealName} onChange={e => setMealName(e.target.value)}/>
                            <label >Meal name</label>
                        </div>
                        <div className="form-floating input-row">
                            <input className="form-control" placeholder="Opis" value={mealDescription} onChange={e => setMealDescription(e.target.value)}/>
                            <label >Meal description</label>
                        </div>
                        <div className="form-floating input-row">
                            <input className="form-control" placeholder="Cena" value={mealPrice} onChange={e => setMealPrice(e.target.value)}/>
                            <label >Meal price</label>
                        </div>
                    </div>
                    <button placeholder="Add meal" onClick={handleOnClick}>Add</button>
                </div>
            </form>
        </div>
    );
}

export default AddRestaurant;