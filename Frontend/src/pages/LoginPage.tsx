
// Import svg from assets/logo.svg
import logo from '../assets/logo.svg'

const LoginPage = () => {
    return (
        <div className="flex justify-center items-center h-screen w-screen">
            <img className="w-1/6" src={logo} alt="Logo ClearTheSky"/>
        </div>
    )

}

export default LoginPage