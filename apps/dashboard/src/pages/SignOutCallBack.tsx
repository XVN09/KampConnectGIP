const SignOutCallBack = () => {
    return (
        <div>
            <p>U bent uitgelogd.</p>
            <button onClick={() => window.location.href = "/"}>Terug naar start</button>
        </div>
    );
};

export default SignOutCallBack;