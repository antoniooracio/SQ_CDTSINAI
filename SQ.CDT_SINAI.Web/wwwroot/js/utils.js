$(document).ready(function() {
    // --- 1. Configuração de Máscaras (jQuery Mask Plugin) ---
    // Aplica máscaras automaticamente em campos com estas classes
    
    // CPF
    $('.mask-cpf').mask('000.000.000-00', {reverse: true});
    
    // CNPJ
    $('.mask-cnpj').mask('00.000.000/0000-00', {reverse: true});
    
    // CEP
    $('.mask-cep').mask('00000-000');
    
    // Telefone (8 ou 9 dígitos)
    var behavior = function (val) {
        return val.replace(/\D/g, '').length === 11 ? '(00) 00000-0000' : '(00) 0000-00009';
    },
    options = {
        onKeyPress: function(val, e, field, options) {
            field.mask(behavior.apply({}, arguments), options);
        }
    };
    $('.mask-phone').mask(behavior, options);

    // Dinheiro
    $('.mask-money').mask('R$ #.##0,00', {reverse: true});

    // Data
    $('.mask-date').mask('00/00/0000');

    // --- 2. Validador de CPF Global ---
    window.validarCPF = function(cpf) {
        cpf = cpf.replace(/[^\d]+/g, '');
        if (cpf == '') return false;
        // Elimina CPFs invalidos conhecidos
        if (cpf.length != 11 ||
            cpf == "00000000000" ||
            cpf == "11111111111" ||
            cpf == "22222222222" ||
            cpf == "33333333333" ||
            cpf == "44444444444" ||
            cpf == "55555555555" ||
            cpf == "66666666666" ||
            cpf == "77777777777" ||
            cpf == "88888888888" ||
            cpf == "99999999999")
            return false;
        // Valida 1o digito
        var add = 0;
        for (var i = 0; i < 9; i++)
            add += parseInt(cpf.charAt(i)) * (10 - i);
        var rev = 11 - (add % 11);
        if (rev == 10 || rev == 11)
            rev = 0;
        if (rev != parseInt(cpf.charAt(9)))
            return false;
        // Valida 2o digito
        add = 0;
        for (var i = 0; i < 10; i++)
            add += parseInt(cpf.charAt(i)) * (11 - i);
        rev = 11 - (add % 11);
        if (rev == 10 || rev == 11)
            rev = 0;
        if (rev != parseInt(cpf.charAt(10)))
            return false;
        return true;
    };

    // --- 3. Integração com jQuery Validation (Client-Side) ---
    // Adiciona método de validação de CPF se o plugin de validação do ASP.NET estiver carregado
    if (typeof $.validator !== 'undefined') {
        $.validator.addMethod("cpf", function(value, element) {
            return this.optional(element) || window.validarCPF(value);
        }, "CPF inválido.");

        // Aplica a regra automaticamente a qualquer campo com a classe .mask-cpf
        $.validator.addClassRules("mask-cpf", {
            cpf: true
        });

        // Validação de Data (DD/MM/AAAA)
        $.validator.addMethod("dateBR", function(value, element) {
            if(this.optional(element)) return true;
            var check = false;
            var re = /^\d{1,2}\/\d{1,2}\/\d{4}$/;
            if( re.test(value)){
                var adata = value.split('/');
                var gg = parseInt(adata[0],10);
                var mm = parseInt(adata[1],10);
                var aaaa = parseInt(adata[2],10);
                var xdata = new Date(aaaa,mm-1,gg);
                if ( ( xdata.getFullYear() === aaaa ) && ( xdata.getMonth () === mm - 1 ) && ( xdata.getDate() === gg ) )
                    check = true;
                else
                    check = false;
            } else
                check = false;
            return check;
        }, "Data inválida.");

        // Aplica a regra automaticamente a qualquer campo com a classe .mask-date
        $.validator.addClassRules("mask-date", {
            dateBR: true
        });
    }

    // --- 4. Busca de CEP Automática ---
    // Funciona para qualquer campo com a classe .mask-cep
    // Tenta preencher campos com IDs padrões usados no sistema (AddressStreet ou Address, etc.)
    $(document).on('blur', '.mask-cep', function() {
        var cep = $(this).val().replace(/\D/g, '');
        if (cep != "") {
            var validacep = /^[0-9]{8}$/;
            if(validacep.test(cep)) {
                // Mapeamento de campos (suporta nomes usados em Colaborador e Estabelecimento)
                var fields = {
                    street: $("#AddressStreet, #Address"),
                    neighborhood: $("#AddressNeighborhood, #Neighborhood"),
                    city: $("#AddressCity, #City"),
                    state: $("#AddressState, #State"),
                    number: $("#AddressNumber")
                };

                fields.street.val("...");
                fields.neighborhood.val("...");
                fields.city.val("...");
                fields.state.val("...");

                $.getJSON("https://viacep.com.br/ws/"+ cep +"/json/?callback=?", function(dados) {
                    if (!("erro" in dados)) {
                        fields.street.val(dados.logradouro);
                        fields.neighborhood.val(dados.bairro);
                        fields.city.val(dados.localidade);
                        fields.state.val(dados.uf);
                        if(fields.number.length) fields.number.focus();
                    } else {
                        alert("CEP não encontrado.");
                        fields.street.val("");
                        fields.neighborhood.val("");
                        fields.city.val("");
                        fields.state.val("");
                    }
                });
            } else {
                alert("Formato de CEP inválido.");
            }
        }
    });

    // --- 5. Toastr Helper (Função Global) ---
    // Uso: showAlert('success', 'Salvo com sucesso!', 'Sucesso');
    window.showAlert = function(type, message, title) {
        if (typeof toastr !== 'undefined') {
            toastr.options = {
                "closeButton": true,
                "progressBar": true,
                "positionClass": "toast-top-right",
                "timeOut": "5000"
            };
            if (['success', 'info', 'warning', 'error'].indexOf(type) === -1) type = 'info';
            toastr[type](message, title);
        } else {
            alert((title ? title + ": " : "") + message);
        }
    };
});