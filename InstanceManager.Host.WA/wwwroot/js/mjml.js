import mjml2html from 'https://cdn.jsdelivr.net/npm/mjml-browser@4.15.3/+esm';

window.mjml = function (mjmlContent, options = {}) {
    try {
        const result = mjml2html(mjmlContent, {
            validationLevel: options.validationLevel || 'soft',
            ...options
        });

        return {
            html: result.html,
            errors: result.errors || []
        };
    } catch (error) {
        return {
            html: '',
            errors: [{
                message: error.message || 'Unknown error occurred',
                tagName: '',
                formattedMessage: error.toString()
            }]
        };
    }
};

export default {};
