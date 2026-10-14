import { Link } from "react-router";

const NotFoundPage = () => {
    return (
        <>
            <h1>404 - Page Not Found</h1>
            <Link to="/">
                Go back to Home
            </Link>
        </>
    );
}

export default NotFoundPage;
