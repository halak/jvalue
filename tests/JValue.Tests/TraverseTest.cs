﻿using NUnit.Framework;

namespace Halak
{
    /// <summary>
    /// Test case from https://github.com/nst/JSONTestSuite
    /// Generate via https://gist.github.com/halak/3f1dba374038f801ac5625c46a2f575a
    /// </summary>
    /// <seealso cref="http://seriot.ch/parsing_json.php"/>
    public class TraverseTest
    {
        [TestCase("[123.456e-789]", Description = "i_number_double_huge_neg_exp")]
        [TestCase("[0.4e00669999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999969999999006]", Description = "i_number_huge_exp")]
        [TestCase("[-1e+9999]", Description = "i_number_neg_int_huge_exp")]
        [TestCase("[1.5e+9999]", Description = "i_number_pos_double_huge_exp")]
        [TestCase("[-123123e100000]", Description = "i_number_real_neg_overflow")]
        [TestCase("[123123e100000]", Description = "i_number_real_pos_overflow")]
        [TestCase("[123e-10000000]", Description = "i_number_real_underflow")]
        [TestCase("[-123123123123123123123123123123]", Description = "i_number_too_big_neg_int")]
        [TestCase("[100000000000000000000]", Description = "i_number_too_big_pos_int")]
        [TestCase("[-237462374673276894279832749832423479823246327846]", Description = "i_number_very_big_negative_int")]
        [TestCase("{\"\\uDFAA\":0}", Description = "i_object_key_lone_2nd_surrogate")]
        [TestCase("[\"\\uDADA\"]", Description = "i_string_1st_surrogate_but_2nd_missing")]
        [TestCase("[\"\\uD888\\u1234\"]", Description = "i_string_1st_valid_surrogate_2nd_invalid")]
        [TestCase("[\"\\uD800\\n\"]", Description = "i_string_incomplete_surrogate_and_escape_valid")]
        [TestCase("[\"\\uDd1ea\"]", Description = "i_string_incomplete_surrogate_pair")]
        [TestCase("[\"\\uD800\\uD800\\n\"]", Description = "i_string_incomplete_surrogates_escape_valid")]
        [TestCase("[\"\\ud800\"]", Description = "i_string_invalid_lonely_surrogate")]
        [TestCase("[\"\\ud800abc\"]", Description = "i_string_invalid_surrogate")]
        [TestCase("[\"\\uDd1e\\uD834\"]", Description = "i_string_inverted_surrogates_U+1D11E")]
        [TestCase("[\"\\uDFAA\"]", Description = "i_string_lone_second_surrogate")]
        [TestCase("[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]", Description = "i_structure_500_nested_arrays")]
        [TestCase("﻿{}", Description = "i_structure_UTF-8_BOM_empty_object")]
        [TestCase("[1 true]", Description = "n_array_1_true_without_comma")]
        [TestCase("[\"\": 1]", Description = "n_array_colon_instead_of_comma")]
        [TestCase("[\"\"],", Description = "n_array_comma_after_close")]
        [TestCase("[,1]", Description = "n_array_comma_and_number")]
        [TestCase("[1,,2]", Description = "n_array_double_comma")]
        [TestCase("[\"x\",,]", Description = "n_array_double_extra_comma")]
        [TestCase("[\"x\"]]", Description = "n_array_extra_close")]
        [TestCase("[\"\",]", Description = "n_array_extra_comma")]
        [TestCase("[\"x\"", Description = "n_array_incomplete")]
        [TestCase("[x", Description = "n_array_incomplete_invalid_value")]
        [TestCase("[3[4]]", Description = "n_array_inner_array_no_comma")]
        [TestCase("[1:2]", Description = "n_array_items_separated_by_semicolon")]
        [TestCase("[,]", Description = "n_array_just_comma")]
        [TestCase("[-]", Description = "n_array_just_minus")]
        [TestCase("[   , \"\"]", Description = "n_array_missing_value")]
        [TestCase("[\"a\",\n4\n,1,", Description = "n_array_newlines_unclosed")]
        [TestCase("[1,]", Description = "n_array_number_and_comma")]
        [TestCase("[1,,]", Description = "n_array_number_and_several_commas")]
        [TestCase("[\"a\"\\f]", Description = "n_array_spaces_vertical_tab_formfeed")]
        [TestCase("[*]", Description = "n_array_star_inside")]
        [TestCase("[\"\"", Description = "n_array_unclosed")]
        [TestCase("[1,", Description = "n_array_unclosed_trailing_comma")]
        [TestCase("[1,\n1\n,1", Description = "n_array_unclosed_with_new_lines")]
        [TestCase("[{}", Description = "n_array_unclosed_with_object_inside")]
        [TestCase("[fals]", Description = "n_incomplete_false")]
        [TestCase("[nul]", Description = "n_incomplete_null")]
        [TestCase("[tru]", Description = "n_incomplete_true")]
        [TestCase("123 ", Description = "n_multidigit_number_then_00")]
        [TestCase("[++1234]", Description = "n_number_++")]
        [TestCase("[+1]", Description = "n_number_+1")]
        [TestCase("[+Inf]", Description = "n_number_+Inf")]
        [TestCase("[-01]", Description = "n_number_-01")]
        [TestCase("[-1.0.]", Description = "n_number_-1.0.")]
        [TestCase("[-2.]", Description = "n_number_-2.")]
        [TestCase("[-NaN]", Description = "n_number_-NaN")]
        [TestCase("[.-1]", Description = "n_number_.-1")]
        [TestCase("[.2e-3]", Description = "n_number_.2e-3")]
        [TestCase("[0.1.2]", Description = "n_number_0.1.2")]
        [TestCase("[0.3e+]", Description = "n_number_0.3e+")]
        [TestCase("[0.3e]", Description = "n_number_0.3e")]
        [TestCase("[0.e1]", Description = "n_number_0.e1")]
        [TestCase("[0E+]", Description = "n_number_0_capital_E+")]
        [TestCase("[0E]", Description = "n_number_0_capital_E")]
        [TestCase("[0e+]", Description = "n_number_0e+")]
        [TestCase("[0e]", Description = "n_number_0e")]
        [TestCase("[1.0e+]", Description = "n_number_1.0e+")]
        [TestCase("[1.0e-]", Description = "n_number_1.0e-")]
        [TestCase("[1.0e]", Description = "n_number_1.0e")]
        [TestCase("[1 000.0]", Description = "n_number_1_000")]
        [TestCase("[1eE2]", Description = "n_number_1eE2")]
        [TestCase("[2.e+3]", Description = "n_number_2.e+3")]
        [TestCase("[2.e-3]", Description = "n_number_2.e-3")]
        [TestCase("[2.e3]", Description = "n_number_2.e3")]
        [TestCase("[9.e+]", Description = "n_number_9.e+")]
        [TestCase("[1+2]", Description = "n_number_expression")]
        [TestCase("[0x1]", Description = "n_number_hex_1_digit")]
        [TestCase("[0x42]", Description = "n_number_hex_2_digits")]
        [TestCase("[Inf]", Description = "n_number_Inf")]
        [TestCase("[Infinity]", Description = "n_number_infinity")]
        [TestCase("[0e+-1]", Description = "n_number_invalid+-")]
        [TestCase("[-123.123foo]", Description = "n_number_invalid-negative-real")]
        [TestCase("[-Infinity]", Description = "n_number_minus_infinity")]
        [TestCase("[-foo]", Description = "n_number_minus_sign_with_trailing_garbage")]
        [TestCase("[- 1]", Description = "n_number_minus_space_1")]
        [TestCase("[NaN]", Description = "n_number_NaN")]
        [TestCase("[-012]", Description = "n_number_neg_int_starting_with_zero")]
        [TestCase("[-.123]", Description = "n_number_neg_real_without_int_part")]
        [TestCase("[-1x]", Description = "n_number_neg_with_garbage_at_end")]
        [TestCase("[1ea]", Description = "n_number_real_garbage_after_e")]
        [TestCase("[1.]", Description = "n_number_real_without_fractional_part")]
        [TestCase("[.123]", Description = "n_number_starting_with_dot")]
        [TestCase("[１]", Description = "n_number_U+FF11_fullwidth_digit_one")]
        [TestCase("[1.2a-3]", Description = "n_number_with_alpha")]
        [TestCase("[1.8011670033376514H-308]", Description = "n_number_with_alpha_char")]
        [TestCase("[012]", Description = "n_number_with_leading_zero")]
        [TestCase("[\"x\", truth]", Description = "n_object_bad_value")]
        [TestCase("{[: \"x\"}\n", Description = "n_object_bracket_key")]
        [TestCase("{\"x\", null}", Description = "n_object_comma_instead_of_colon")]
        [TestCase("{\"x\"::\"b\"}", Description = "n_object_double_colon")]
        [TestCase("{🇨🇭}", Description = "n_object_emoji")]
        [TestCase("{\"a\":\"a\" 123}", Description = "n_object_garbage_at_end")]
        [TestCase("{key: 'value'}", Description = "n_object_key_with_single_quotes")]
        [TestCase("{\"a\" b}", Description = "n_object_missing_colon")]
        [TestCase("{:\"b\"}", Description = "n_object_missing_key")]
        [TestCase("{\"a\" \"b\"}", Description = "n_object_missing_semicolon")]
        [TestCase("{\"a\":", Description = "n_object_missing_value")]
        [TestCase("{\"a\"", Description = "n_object_no-colon")]
        [TestCase("{1:1}", Description = "n_object_non_string_key")]
        [TestCase("{9999E9999:1}", Description = "n_object_non_string_key_but_huge_number_instead")]
        [TestCase("{null:null,null:null}", Description = "n_object_repeated_null_null")]
        [TestCase("{\"id\":0,,,,,}", Description = "n_object_several_trailing_commas")]
        [TestCase("{'a':0}", Description = "n_object_single_quote")]
        [TestCase("{\"id\":0,}", Description = "n_object_trailing_comma")]
        [TestCase("{\"a\":\"b\"}/**/", Description = "n_object_trailing_comment")]
        [TestCase("{\"a\":\"b\"}/**//", Description = "n_object_trailing_comment_open")]
        [TestCase("{\"a\":\"b\"}//", Description = "n_object_trailing_comment_slash_open")]
        [TestCase("{\"a\":\"b\"}/", Description = "n_object_trailing_comment_slash_open_incomplete")]
        [TestCase("{\"a\":\"b\",,\"c\":\"d\"}", Description = "n_object_two_commas_in_a_row")]
        [TestCase("{a: \"b\"}", Description = "n_object_unquoted_key")]
        [TestCase("{\"a\":\"a", Description = "n_object_unterminated-value")]
        [TestCase("{ \"foo\" : \"bar\", \"a\" }", Description = "n_object_with_single_string")]
        [TestCase("{\"a\":\"b\"}#", Description = "n_object_with_trailing_garbage")]
        [TestCase(" ", Description = "n_single_space")]
        [TestCase("[\"\\uD800\\\"]", Description = "n_string_1_surrogate_then_escape")]
        [TestCase("[\"\\uD800\\u\"]", Description = "n_string_1_surrogate_then_escape_u")]
        [TestCase("[\"\\uD800\\u1\"]", Description = "n_string_1_surrogate_then_escape_u1")]
        [TestCase("[\"\\uD800\\u1x\"]", Description = "n_string_1_surrogate_then_escape_u1x")]
        [TestCase("[é]", Description = "n_string_accentuated_char_no_quotes")]
        [TestCase("[\"\\ \"]", Description = "n_string_backslash_00")]
        [TestCase("[\"\\x00\"]", Description = "n_string_escape_x")]
        [TestCase("[\"\\\\\\\"]", Description = "n_string_escaped_backslash_bad")]
        [TestCase("[\"\\ \"]", Description = "n_string_escaped_ctrl_char_tab")]
        [TestCase("[\"\\🌀\"]", Description = "n_string_escaped_emoji")]
        [TestCase("[\"\\\"]", Description = "n_string_incomplete_escape")]
        [TestCase("[\"\\u00A\"]", Description = "n_string_incomplete_escaped_character")]
        [TestCase("[\"\\uD834\\uDd\"]", Description = "n_string_incomplete_surrogate")]
        [TestCase("[\"\\uD800\\uD800\\x\"]", Description = "n_string_incomplete_surrogate_escape_invalid")]
        [TestCase("[\"\\a\"]", Description = "n_string_invalid_backslash_esc")]
        [TestCase("[\"\\uqqqq\"]", Description = "n_string_invalid_unicode_escape")]
        [TestCase("[\\u0020\"asd\"]", Description = "n_string_leading_uescaped_thinspace")]
        [TestCase("[\\n]", Description = "n_string_no_quotes_with_bad_escape")]
        [TestCase("\"", Description = "n_string_single_doublequote")]
        [TestCase("['single quote']", Description = "n_string_single_quote")]
        [TestCase("abc", Description = "n_string_single_string_no_double_quotes")]
        [TestCase("[\"\\", Description = "n_string_start_escape_unclosed")]
        [TestCase("[\"a a\"]", Description = "n_string_unescaped_ctrl_char")]
        [TestCase("[\"new\nline\"]", Description = "n_string_unescaped_newline")]
        [TestCase("[\"   \"]", Description = "n_string_unescaped_tab")]
        [TestCase("\"\\UA66D\"", Description = "n_string_unicode_CapitalU")]
        [TestCase("\"\"x", Description = "n_string_with_trailing_garbage")]
        [TestCase("<.>", Description = "n_structure_angle_bracket_.")]
        [TestCase("[<null>]", Description = "n_structure_angle_bracket_null")]
        [TestCase("[1]x", Description = "n_structure_array_trailing_garbage")]
        [TestCase("[1]]", Description = "n_structure_array_with_extra_array_close")]
        [TestCase("[\"asd]", Description = "n_structure_array_with_unclosed_string")]
        [TestCase("aå", Description = "n_structure_ascii-unicode-identifier")]
        [TestCase("[True]", Description = "n_structure_capitalized_True")]
        [TestCase("1]", Description = "n_structure_close_unopened_array")]
        [TestCase("{\"x\": true,", Description = "n_structure_comma_instead_of_closing_brace")]
        [TestCase("[][]", Description = "n_structure_double_array")]
        [TestCase("]", Description = "n_structure_end_array")]
        [TestCase("[", Description = "n_structure_lone-open-bracket")]
        [TestCase("", Description = "n_structure_no_data")]
        [TestCase("[ ]", Description = "n_structure_null-byte-outside-string")]
        [TestCase("2@", Description = "n_structure_number_with_trailing_garbage")]
        [TestCase("{}}", Description = "n_structure_object_followed_by_closing_object")]
        [TestCase("{\"\":", Description = "n_structure_object_unclosed_no_value")]
        [TestCase("{\"a\":/*comment*/\"b\"}", Description = "n_structure_object_with_comment")]
        [TestCase("{\"a\": true} \"x\"", Description = "n_structure_object_with_trailing_garbage")]
        [TestCase("['", Description = "n_structure_open_array_apostrophe")]
        [TestCase("[,", Description = "n_structure_open_array_comma")]
        [TestCase("[{", Description = "n_structure_open_array_open_object")]
        [TestCase("[\"a", Description = "n_structure_open_array_open_string")]
        [TestCase("[\"a\"", Description = "n_structure_open_array_string")]
        [TestCase("{", Description = "n_structure_open_object")]
        [TestCase("{]", Description = "n_structure_open_object_close_array")]
        [TestCase("{,", Description = "n_structure_open_object_comma")]
        [TestCase("{[", Description = "n_structure_open_object_open_array")]
        [TestCase("{\"a", Description = "n_structure_open_object_open_string")]
        [TestCase("{'a'", Description = "n_structure_open_object_string_with_apostrophes")]
        [TestCase("[\"\\{[\"\\{[\"\\{[\"\\{", Description = "n_structure_open_open")]
        [TestCase("*", Description = "n_structure_single_star")]
        [TestCase("{\"a\":\"b\"}#{}", Description = "n_structure_trailing_#")]
        [TestCase("[⁠]", Description = "n_structure_U+2060_word_joined")]
        [TestCase("[\\u000A\"\"]", Description = "n_structure_uescaped_LF_before_string")]
        [TestCase("[1", Description = "n_structure_unclosed_array")]
        [TestCase("[ false, nul", Description = "n_structure_unclosed_array_partial_null")]
        [TestCase("[ true, fals", Description = "n_structure_unclosed_array_unfinished_false")]
        [TestCase("[ false, tru", Description = "n_structure_unclosed_array_unfinished_true")]
        [TestCase("{\"asd\":\"asd\"", Description = "n_structure_unclosed_object")]
        [TestCase("å", Description = "n_structure_unicode-identifier")]
        [TestCase("﻿", Description = "n_structure_UTF8_BOM_no_data")]
        [TestCase("[]", Description = "n_structure_whitespace_formfeed")]
        [TestCase("[⁠]", Description = "n_structure_whitespace_U+2060_word_joiner")]
        [TestCase("[-9223372036854775808]\n", Description = "number_-9223372036854775808")]
        [TestCase("[-9223372036854775809]\n", Description = "number_-9223372036854775809")]
        [TestCase("[1.0]\n", Description = "number_1.0")]
        [TestCase("[1.000000000000000005]\n", Description = "number_1.000000000000000005")]
        [TestCase("[1000000000000000]\n", Description = "number_1000000000000000")]
        [TestCase("[10000000000000000999]\n", Description = "number_10000000000000000999")]
        [TestCase("[1E-999]\n", Description = "number_1e-999")]
        [TestCase("[1E6]\n", Description = "number_1e6")]
        [TestCase("[9223372036854775807]\n", Description = "number_9223372036854775807")]
        [TestCase("[9223372036854775808]\n", Description = "number_9223372036854775808")]
        [TestCase("{\"é\":\"NFC\",\"é\":\"NFD\"}", Description = "object_key_nfc_nfd")]
        [TestCase("{\"é\":\"NFD\",\"é\":\"NFC\"}", Description = "object_key_nfd_nfc")]
        [TestCase("{\"a\":1,\"a\":2}", Description = "object_same_key_different_values")]
        [TestCase("{\"a\":1,\"a\":1}", Description = "object_same_key_same_value")]
        [TestCase("{\"a\":0, \"a\":-0}\n", Description = "object_same_key_unclear_values")]
        [TestCase("[\"\\uD800\"]", Description = "string_1_escaped_invalid_codepoint")]
        [TestCase("[\"\\uD800\\uD800\"]", Description = "string_2_escaped_invalid_codepoints")]
        [TestCase("[\"\\uD800\\uD800\\uD800\"]", Description = "string_3_escaped_invalid_codepoints")]
        [TestCase("[\"A\\u0000B\"]", Description = "string_with_escaped_NULL")]
        [TestCase("[[]   ]", Description = "y_array_arraysWithSpaces")]
        [TestCase("[\"\"]", Description = "y_array_empty-string")]
        [TestCase("[]", Description = "y_array_empty")]
        [TestCase("[\"a\"]", Description = "y_array_ending_with_newline")]
        [TestCase("[false]", Description = "y_array_false")]
        [TestCase("[null, 1, \"1\", {}]", Description = "y_array_heterogeneous")]
        [TestCase("[null]", Description = "y_array_null")]
        [TestCase("[1\n]", Description = "y_array_with_1_and_newline")]
        [TestCase(" [1]", Description = "y_array_with_leading_space")]
        [TestCase("[1,null,null,null,2]", Description = "y_array_with_several_null")]
        [TestCase("[2] ", Description = "y_array_with_trailing_space")]
        [TestCase("[123e65]", Description = "y_number")]
        [TestCase("[0e+1]", Description = "y_number_0e+1")]
        [TestCase("[0e1]", Description = "y_number_0e1")]
        [TestCase("[ 4]", Description = "y_number_after_space")]
        [TestCase("[-0.000000000000000000000000000000000000000000000000000000000000000000000000000001]\n", Description = "y_number_double_close_to_zero")]
        [TestCase("[20e1]", Description = "y_number_int_with_exp")]
        [TestCase("[-0]", Description = "y_number_minus_zero")]
        [TestCase("[-123]", Description = "y_number_negative_int")]
        [TestCase("[-1]", Description = "y_number_negative_one")]
        [TestCase("[-0]", Description = "y_number_negative_zero")]
        [TestCase("[1E22]", Description = "y_number_real_capital_e")]
        [TestCase("[1E-2]", Description = "y_number_real_capital_e_neg_exp")]
        [TestCase("[1E+2]", Description = "y_number_real_capital_e_pos_exp")]
        [TestCase("[123e45]", Description = "y_number_real_exponent")]
        [TestCase("[123.456e78]", Description = "y_number_real_fraction_exponent")]
        [TestCase("[1e-2]", Description = "y_number_real_neg_exp")]
        [TestCase("[1e+2]", Description = "y_number_real_pos_exponent")]
        [TestCase("[123]", Description = "y_number_simple_int")]
        [TestCase("[123.456789]", Description = "y_number_simple_real")]
        [TestCase("{\"asd\":\"sdf\", \"dfg\":\"fgh\"}", Description = "y_object")]
        [TestCase("{\"asd\":\"sdf\"}", Description = "y_object_basic")]
        [TestCase("{\"a\":\"b\",\"a\":\"c\"}", Description = "y_object_duplicated_key")]
        [TestCase("{\"a\":\"b\",\"a\":\"b\"}", Description = "y_object_duplicated_key_and_value")]
        [TestCase("{}", Description = "y_object_empty")]
        [TestCase("{\"\":0}", Description = "y_object_empty_key")]
        [TestCase("{\"foo\\u0000bar\": 42}", Description = "y_object_escaped_null_in_key")]
        [TestCase("{ \"min\": -1.0e+28, \"max\": 1.0e+28 }", Description = "y_object_extreme_numbers")]
        [TestCase("{\"x\":[{\"id\": \"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\"}], \"id\": \"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\"}", Description = "y_object_long_strings")]
        [TestCase("{\"a\":[]}", Description = "y_object_simple")]
        [TestCase("{\"title\":\"\\u041f\\u043e\\u043b\\u0442\\u043e\\u0440\\u0430 \\u0417\\u0435\\u043c\\u043b\\u0435\\u043a\\u043e\\u043f\\u0430\" }", Description = "y_object_string_unicode")]
        [TestCase("{\n\"a\": \"b\"\n}", Description = "y_object_with_newlines")]
        [TestCase("[\"\\u0060\\u012a\\u12AB\"]", Description = "y_string_1_2_3_bytes_UTF-8_sequences")]
        [TestCase("[\"\\uD801\\udc37\"]", Description = "y_string_accepted_surrogate_pair")]
        [TestCase("[\"\\ud83d\\ude39\\ud83d\\udc8d\"]", Description = "y_string_accepted_surrogate_pairs")]
        [TestCase("[\"\\\"\\\\\\/\\b\\f\\n\\r\\t\"]", Description = "y_string_allowed_escapes")]
        [TestCase("[\"\\\\u0000\"]", Description = "y_string_backslash_and_u_escaped_zero")]
        [TestCase("[\"\\\"\"]", Description = "y_string_backslash_doublequotes")]
        [TestCase("[\"a/*b*/c/*d//e\"]", Description = "y_string_comments")]
        [TestCase("[\"\\\\a\"]", Description = "y_string_double_escape_a")]
        [TestCase("[\"\\\\n\"]", Description = "y_string_double_escape_n")]
        [TestCase("[\"\\u0012\"]", Description = "y_string_escaped_control_character")]
        [TestCase("[\"\\uFFFF\"]", Description = "y_string_escaped_noncharacter")]
        [TestCase("[\"asd\"]", Description = "y_string_in_array")]
        [TestCase("[ \"asd\"]", Description = "y_string_in_array_with_leading_space")]
        [TestCase("[\"\\uDBFF\\uDFFF\"]", Description = "y_string_last_surrogates_1_and_2")]
        [TestCase("[\"new\\u00A0line\"]", Description = "y_string_nbsp_uescaped")]
        [TestCase("[\"􏿿\"]", Description = "y_string_nonCharacterInUTF-8_U+10FFFF")]
        [TestCase("[\"￿\"]", Description = "y_string_nonCharacterInUTF-8_U+FFFF")]
        [TestCase("[\"\\u0000\"]", Description = "y_string_null_escape")]
        [TestCase("[\"\\u002c\"]", Description = "y_string_one-byte-utf-8")]
        [TestCase("[\"π\"]", Description = "y_string_pi")]
        [TestCase("[\"𛿿\"]", Description = "y_string_reservedCharacterInUTF-8_U+1BFFF")]
        [TestCase("[\"asd \"]", Description = "y_string_simple_ascii")]
        [TestCase("\" \"", Description = "y_string_space")]
        [TestCase("[\"\\uD834\\uDd1e\"]", Description = "y_string_surrogates_U+1D11E_MUSICAL_SYMBOL_G_CLEF")]
        [TestCase("[\"\\u0821\"]", Description = "y_string_three-byte-utf-8")]
        [TestCase("[\"\\u0123\"]", Description = "y_string_two-byte-utf-8")]
        [TestCase("[\"\\u2028\"]", Description = "y_string_u+2028_line_sep")]
        [TestCase("[\"\\u2029\"]", Description = "y_string_u+2029_par_sep")]
        [TestCase("[\"\\u0061\\u30af\\u30EA\\u30b9\"]", Description = "y_string_uEscape")]
        [TestCase("[\"new\\u000Aline\"]", Description = "y_string_uescaped_newline")]
        [TestCase("[\"\"]", Description = "y_string_unescaped_char_delete")]
        [TestCase("[\"\\uA66D\"]", Description = "y_string_unicode")]
        [TestCase("[\"⍂㈴⍂\"]", Description = "y_string_unicode_2")]
        [TestCase("[\"\\u0022\"]", Description = "y_string_unicode_escaped_double_quote")]
        [TestCase("[\"\\uDBFF\\uDFFE\"]", Description = "y_string_unicode_U+10FFFE_nonchar")]
        [TestCase("[\"\\uD83F\\uDFFE\"]", Description = "y_string_unicode_U+1FFFE_nonchar")]
        [TestCase("[\"\\u200B\"]", Description = "y_string_unicode_U+200B_ZERO_WIDTH_SPACE")]
        [TestCase("[\"\\u2064\"]", Description = "y_string_unicode_U+2064_invisible_plus")]
        [TestCase("[\"\\uFDD0\"]", Description = "y_string_unicode_U+FDD0_nonchar")]
        [TestCase("[\"\\uFFFE\"]", Description = "y_string_unicode_U+FFFE_nonchar")]
        [TestCase("[\"\\u005C\"]", Description = "y_string_unicodeEscapedBackslash")]
        [TestCase("[\"€𝄞\"]", Description = "y_string_utf8")]
        [TestCase("[\"aa\"]", Description = "y_string_with_del_character")]
        [TestCase("false", Description = "y_structure_lonely_false")]
        [TestCase("42", Description = "y_structure_lonely_int")]
        [TestCase("-0.1", Description = "y_structure_lonely_negative_real")]
        [TestCase("null", Description = "y_structure_lonely_null")]
        [TestCase("\"asd\"", Description = "y_structure_lonely_string")]
        [TestCase("true", Description = "y_structure_lonely_true")]
        [TestCase("\"\"", Description = "y_structure_string_empty")]
        [TestCase("[\"a\"]\n", Description = "y_structure_trailing_newline")]
        [TestCase("[true]", Description = "y_structure_true_in_array")]
        [TestCase(" [] ", Description = "y_structure_whitespace_array")]
        public void TraverseJson(string source)
        {
            Traverse(JValue.Parse(source));

            static void Traverse(JValue json)
            {
                switch (json.Type)
                {
                    case JValue.TypeCode.Null:
                        break;
                    case JValue.TypeCode.Boolean:
                        json.ToBoolean();
                        break;
                    case JValue.TypeCode.Number:
                        json.ToNumber().ToInt32();
                        json.ToNumber().ToInt64();
                        json.ToNumber().ToSingle();
                        json.ToNumber().ToDouble();
                        json.ToNumber().ToDecimal();
                        break;
                    case JValue.TypeCode.String:
                        json.ToUnescapedString();
                        break;
                    case JValue.TypeCode.Array:
                        foreach (var element in json.Array())
                            Traverse(element);
                        break;
                    case JValue.TypeCode.Object:
                        foreach (var member in json.Object())
                        {
                            Traverse(member.Key);
                            Traverse(member.Value);
                        }
                        break;
                }
            }
        }
    }
}
