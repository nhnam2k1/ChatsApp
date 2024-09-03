import Background from './background.png';

const Home = () => {
    return(
        <div style={style}>
            <img src={Background} style={imgStyle} alt="This is img"/>
        </div>
    )
}

const style = {
    margin: "15px"
}

const imgStyle = {
    maxWidth: "100%",
    height: "auto"
}
export default Home;